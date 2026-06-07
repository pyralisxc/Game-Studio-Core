using System.Collections;
using System.Reflection;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public class ProjectileRuntimePlayModeTests
    {
        [UnityTest]
        public IEnumerator ProjectileLauncher3D_PooledRuntimeProjectile_ReturnsAndReusesAfterLifetime()
        {
            GameObject launcherObject = new GameObject("Launcher3D");
            ProjectileLauncher3D launcher = launcherObject.AddComponent<ProjectileLauncher3D>();
            EnablePooling(launcher);

            GameObject prefab = new GameObject("PooledProjectile3D");
            Rigidbody body = prefab.AddComponent<Rigidbody>();
            body.useGravity = false;
            SphereCollider collider = prefab.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            prefab.AddComponent<Projectile>();
            prefab.SetActive(false);

            ProjectileSpawnCommand firstCommand = new ProjectileSpawnCommand(
                ProjectileDeliveryMode.ProjectilePrefab,
                prefab,
                Vector3.zero,
                Vector3.forward,
                5f,
                0f,
                6f,
                0f,
                0.05f,
                Faction.Player,
                launcherObject,
                0f,
                false);

            ProjectileSpawnResult firstResult = launcher.Execute(firstCommand);
            GameObject firstInstance = firstResult.SpawnedObject;

            Assert.That(firstResult.Status, Is.EqualTo(ProjectileSpawnStatus.Spawned));
            Assert.That(firstInstance, Is.Not.Null);
            Assert.That(firstInstance.activeSelf, Is.True);
            firstInstance.GetComponent<Rigidbody>().angularVelocity = Vector3.one * 12f;

            yield return new WaitForSeconds(0.1f);

            Assert.That(firstInstance == null, Is.False);
            Assert.That(firstInstance.activeSelf, Is.False);
            Assert.That(firstInstance.GetComponent<Rigidbody>().angularVelocity, Is.EqualTo(Vector3.zero));

            ProjectileSpawnCommand secondCommand = new ProjectileSpawnCommand(
                ProjectileDeliveryMode.ProjectilePrefab,
                prefab,
                Vector3.right,
                Vector3.up,
                5f,
                0f,
                3f,
                0f,
                0.05f,
                Faction.Player,
                launcherObject,
                0f,
                false);

            ProjectileSpawnResult secondResult = launcher.Execute(secondCommand);
            GameObject secondInstance = secondResult.SpawnedObject;

            Assert.That(secondResult.Status, Is.EqualTo(ProjectileSpawnStatus.Spawned));
            Assert.That(secondInstance, Is.SameAs(firstInstance));
            Assert.That(secondInstance.activeSelf, Is.True);
            Assert.That(secondInstance.transform.position, Is.EqualTo(Vector3.right));
            Assert.That(secondInstance.GetComponent<Rigidbody>().linearVelocity, Is.EqualTo(Vector3.up * 3f));

            Object.DestroyImmediate(secondInstance);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(launcherObject);
        }

        [UnityTest]
        public IEnumerator ProjectileLauncher2D_PooledRuntimeProjectile_ReturnsAndReusesAfterLifetime()
        {
            GameObject launcherObject = new GameObject("Launcher2D");
            ProjectileLauncher2D launcher = launcherObject.AddComponent<ProjectileLauncher2D>();
            EnablePooling(launcher);

            GameObject prefab = new GameObject("PooledProjectile2D");
            Rigidbody2D body = prefab.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            BoxCollider2D collider = prefab.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            prefab.AddComponent<Projectile2D>();
            prefab.SetActive(false);

            ProjectileSpawnCommand firstCommand = new ProjectileSpawnCommand(
                ProjectileDeliveryMode.ProjectilePrefab,
                prefab,
                Vector3.zero,
                Vector3.right,
                5f,
                0f,
                6f,
                0f,
                0.05f,
                Faction.Player,
                launcherObject,
                0f,
                false);

            ProjectileSpawnResult firstResult = launcher.Execute(firstCommand);
            GameObject firstInstance = firstResult.SpawnedObject;

            Assert.That(firstResult.Status, Is.EqualTo(ProjectileSpawnStatus.Spawned));
            Assert.That(firstInstance, Is.Not.Null);
            Assert.That(firstInstance.activeSelf, Is.True);
            firstInstance.GetComponent<Rigidbody2D>().angularVelocity = 45f;
            Assert.That(firstInstance.GetComponent<Rigidbody2D>().collisionDetectionMode, Is.EqualTo(CollisionDetectionMode2D.Continuous));

            yield return new WaitForSeconds(0.1f);

            Assert.That(firstInstance == null, Is.False);
            Assert.That(firstInstance.activeSelf, Is.False);
            Assert.That(firstInstance.GetComponent<Rigidbody2D>().angularVelocity, Is.EqualTo(0f));

            ProjectileSpawnCommand secondCommand = new ProjectileSpawnCommand(
                ProjectileDeliveryMode.ProjectilePrefab,
                prefab,
                Vector3.up,
                Vector3.left,
                5f,
                0f,
                4f,
                0f,
                0.05f,
                Faction.Player,
                launcherObject,
                0f,
                false);

            ProjectileSpawnResult secondResult = launcher.Execute(secondCommand);
            GameObject secondInstance = secondResult.SpawnedObject;

            Assert.That(secondResult.Status, Is.EqualTo(ProjectileSpawnStatus.Spawned));
            Assert.That(secondInstance, Is.SameAs(firstInstance));
            Assert.That(secondInstance.activeSelf, Is.True);
            Assert.That(secondInstance.transform.position, Is.EqualTo(Vector3.up));
            Assert.That(secondInstance.GetComponent<Rigidbody2D>().linearVelocity, Is.EqualTo(Vector2.left * 4f));

            Object.DestroyImmediate(secondInstance);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(launcherObject);
        }

        [UnityTest]
        public IEnumerator Projectile3D_TriggerDamagesEnemySpawnsImpactAndReturnsToPool()
        {
            GameObject launcherObject = new GameObject("Launcher3D");
            ProjectileLauncher3D launcher = launcherObject.AddComponent<ProjectileLauncher3D>();
            EnablePooling(launcher);

            GameObject prefab = new GameObject("ContactProjectile3D");
            Rigidbody body = prefab.AddComponent<Rigidbody>();
            body.useGravity = false;
            SphereCollider projectileCollider = prefab.AddComponent<SphereCollider>();
            projectileCollider.isTrigger = true;
            prefab.AddComponent<Projectile>();
            prefab.SetActive(false);

            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "ProjectileTarget3D";
            target.transform.position = Vector3.forward;
            HealthComponent health = target.AddComponent<HealthComponent>();
            health.faction = Faction.Enemy;

            GameObject hitEffectPrefab = new GameObject("ProjectileHitEffect3D");
            ProjectileImpactDefinition impact = ScriptableObject.CreateInstance<ProjectileImpactDefinition>();
            impact.hitEffectPrefab = hitEffectPrefab;
            impact.effectLifetime = 0f;

            ProjectileSpawnCommand command = new ProjectileSpawnCommand(
                ProjectileDeliveryMode.ProjectilePrefab,
                prefab,
                Vector3.zero,
                Vector3.forward,
                25f,
                0f,
                6f,
                20f,
                1f,
                Faction.Player,
                launcherObject,
                0f,
                false,
                impact);

            ProjectileSpawnResult result = launcher.Execute(command);
            GameObject instance = result.SpawnedObject;

            InvokePrivateTrigger(instance.GetComponent<Projectile>(), "OnTriggerEnter", target.GetComponent<Collider>());
            yield return null;

            Assert.That(health.CurrentHealth, Is.EqualTo(75f));
            Assert.That(instance == null, Is.False);
            Assert.That(instance.activeSelf, Is.False);
            Assert.That(GameObject.Find("ProjectileHitEffect3D(Clone)"), Is.Not.Null);

            Object.DestroyImmediate(GameObject.Find("ProjectileHitEffect3D(Clone)"));
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(hitEffectPrefab);
            Object.DestroyImmediate(impact);
            Object.DestroyImmediate(instance);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(launcherObject);
        }

        [UnityTest]
        public IEnumerator Projectile2D_TriggerDamagesEnemySpawnsImpactAndReturnsToPool()
        {
            GameObject launcherObject = new GameObject("Launcher2D");
            ProjectileLauncher2D launcher = launcherObject.AddComponent<ProjectileLauncher2D>();
            EnablePooling(launcher);

            GameObject prefab = new GameObject("ContactProjectile2D");
            Rigidbody2D body = prefab.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            BoxCollider2D projectileCollider = prefab.AddComponent<BoxCollider2D>();
            projectileCollider.isTrigger = true;
            prefab.AddComponent<Projectile2D>();
            prefab.SetActive(false);

            GameObject target = new GameObject("ProjectileTarget2D");
            BoxCollider2D targetCollider = target.AddComponent<BoxCollider2D>();
            HealthComponent health = target.AddComponent<HealthComponent>();
            health.faction = Faction.Enemy;

            GameObject hitEffectPrefab = new GameObject("ProjectileHitEffect2D");
            ProjectileImpactDefinition impact = ScriptableObject.CreateInstance<ProjectileImpactDefinition>();
            impact.hitEffectPrefab = hitEffectPrefab;
            impact.effectLifetime = 0f;

            ProjectileSpawnCommand command = new ProjectileSpawnCommand(
                ProjectileDeliveryMode.ProjectilePrefab,
                prefab,
                Vector3.zero,
                Vector3.right,
                30f,
                0f,
                6f,
                20f,
                1f,
                Faction.Player,
                launcherObject,
                0f,
                false,
                impact);

            ProjectileSpawnResult result = launcher.Execute(command);
            GameObject instance = result.SpawnedObject;

            InvokePrivateTrigger(instance.GetComponent<Projectile2D>(), "OnTriggerEnter2D", targetCollider);
            yield return null;

            Assert.That(health.CurrentHealth, Is.EqualTo(70f));
            Assert.That(instance == null, Is.False);
            Assert.That(instance.activeSelf, Is.False);
            Assert.That(GameObject.Find("ProjectileHitEffect2D(Clone)"), Is.Not.Null);

            Object.DestroyImmediate(GameObject.Find("ProjectileHitEffect2D(Clone)"));
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(hitEffectPrefab);
            Object.DestroyImmediate(impact);
            Object.DestroyImmediate(instance);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(launcherObject);
        }

        [UnityTest]
        public IEnumerator ProjectileLauncher_CancelsDelayedCommandsWhenDisabled()
        {
            GameObject launcherObject = new GameObject("Launcher3D");
            ProjectileLauncher3D launcher = launcherObject.AddComponent<ProjectileLauncher3D>();

            GameObject prefab = new GameObject("DelayedProjectile3D");
            Rigidbody body = prefab.AddComponent<Rigidbody>();
            body.useGravity = false;
            SphereCollider collider = prefab.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            prefab.AddComponent<Projectile>();
            prefab.SetActive(false);

            ProjectileSpawnCommand command = new ProjectileSpawnCommand(
                ProjectileDeliveryMode.ProjectilePrefab,
                prefab,
                Vector3.zero,
                Vector3.forward,
                5f,
                0f,
                6f,
                0f,
                0.25f,
                Faction.Player,
                launcherObject,
                0.15f,
                false);

            ProjectileSpawnResult result = launcher.Execute(command);
            launcher.enabled = false;

            yield return new WaitForSeconds(0.25f);

            Assert.That(result.Status, Is.EqualTo(ProjectileSpawnStatus.Pending));
            Assert.That(GameObject.Find("DelayedProjectile3D(Clone)"), Is.Null);

            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(launcherObject);
        }

        private static void EnablePooling(ProjectileLauncherBase launcher)
        {
            SetPrivateField(launcher, "usePrefabPooling", true);
            SetPrivateField(launcher, "maxPoolSizePerPrefab", 4);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = typeof(ProjectileLauncherBase).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected ProjectileLauncherBase private field `{fieldName}`.");
            field.SetValue(target, value);
        }

        private static void InvokePrivateTrigger(object target, string methodName, object collider)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Expected private trigger method `{methodName}`.");
            method.Invoke(target, new[] { collider });
        }
    }
}

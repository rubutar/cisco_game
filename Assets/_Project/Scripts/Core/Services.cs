// SHARED CONTRACT. Change only through a contract-change PR to main (see CLAUDE.md).
using System;
using System.Collections.Generic;
using ByteWorld.Stubs;
using UnityEngine;

namespace ByteWorld
{
    /// <summary>
    /// Tiny service registry. Real implementations call Register in Awake; anything not registered
    /// falls back to a stub, so each stream can run without the other's code.
    /// </summary>
    public static class Services
    {
        static readonly Dictionary<Type, object> registered = new();
        static readonly Dictionary<Type, Func<object>> stubFactories = new()
        {
            { typeof(IBackupNest), () => new StubBackupNest() },
            { typeof(IGameData), () => new ResourcesGameData() },
            { typeof(IGameFlow), () => new StubGameFlow() },
            { typeof(IDialogueService), () => StubBehaviour.Create<StubDialogueService>("Stub Dialogue") },
            { typeof(IInspectPanel), () => StubBehaviour.Create<StubInspectPanel>("Stub Inspect Panel") },
            { typeof(IBuildMode), () => new StubBuildMode() },
            { typeof(IHuntScreen), () => new StubHuntScreen() },
            { typeof(ICheckpointService), () => new StubCheckpointService() },
        };

        public static IBackupNest Backups => Get<IBackupNest>();
        public static IGameData Data => Get<IGameData>();
        public static IGameFlow Flow => Get<IGameFlow>();
        public static IDialogueService Dialogue => Get<IDialogueService>();
        public static IInspectPanel Inspect => Get<IInspectPanel>();
        public static IBuildMode BuildMode => Get<IBuildMode>();
        public static IHuntScreen Hunt => Get<IHuntScreen>();
        public static ICheckpointService Checkpoints => Get<ICheckpointService>();

        public static void Register<T>(T implementation) where T : class
        {
            registered[typeof(T)] = implementation;
        }

        public static void Unregister<T>(T implementation) where T : class
        {
            if (registered.TryGetValue(typeof(T), out var current) && ReferenceEquals(current, implementation))
                registered.Remove(typeof(T));
        }

        public static bool IsStub<T>() where T : class =>
            !registered.TryGetValue(typeof(T), out var impl) || impl is IStub;

        public static T Get<T>() where T : class
        {
            if (registered.TryGetValue(typeof(T), out var impl) && !(impl is UnityEngine.Object o && o == null))
                return (T)impl;

            if (!stubFactories.TryGetValue(typeof(T), out var factory))
                throw new InvalidOperationException($"No implementation or stub for {typeof(T).Name}");

            var stub = (T)factory();
            registered[typeof(T)] = stub;
            Debug.Log($"[Services] Using stub for {typeof(T).Name}");
            return stub;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetAll() => registered.Clear();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    internal static class SingletonRegistry
    {
        private static readonly Dictionary<Type, MonoBehaviour> Instances = new();
        private static readonly HashSet<Type> QuittingTypes = new();

        public static bool TryGet<T>(out T instance) where T : MonoBehaviour
        {
            Type type = typeof(T);

            if (Instances.TryGetValue(type, out MonoBehaviour cached) && cached != null)
            {
                instance = (T)cached;
                return true;
            }

            Instances.Remove(type);
            instance = null;
            return false;
        }

        public static void Set<T>(T instance) where T : MonoBehaviour
        {
            Instances[typeof(T)] = instance;
        }

        public static void Remove<T>(T instance) where T : MonoBehaviour
        {
            Type type = typeof(T);

            if (Instances.TryGetValue(type, out MonoBehaviour cached) && cached == instance)
            {
                Instances.Remove(type);
            }
        }

        public static bool IsQuitting<T>() where T : MonoBehaviour => QuittingTypes.Contains(typeof(T));

        public static void MarkQuitting<T>() where T : MonoBehaviour
        {
            QuittingTypes.Add(typeof(T));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instances.Clear();
            QuittingTypes.Clear();
        }
    }

    /// <summary>
    /// Unity용 제네릭 싱글톤 베이스 클래스.
    /// 같은 타입의 인스턴스가 여러 개 생성되면 중복 오브젝트를 제거합니다.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance
        {
            get
            {
                if (SingletonRegistry.IsQuitting<T>())
                {
                    return null;
                }

                if (!SingletonRegistry.TryGet(out T instance))
                {
                    instance = FindFirstObjectByType<T>();

                    if (instance != null)
                    {
                        SingletonRegistry.Set(instance);
                    }
                }

                return instance;
            }
        }

        public static bool HasInstance => Instance != null;

        /// <summary>
        /// true이면 첫 인스턴스를 씬 전환 후에도 유지합니다.
        /// 기본값은 false 입니다.
        /// </summary>
        protected virtual bool PersistBetweenScenes => false;

        protected virtual void Awake()
        {
            if (SingletonRegistry.TryGet(out T instance) && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            SingletonRegistry.Set((T)this);

            if (PersistBetweenScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            SingletonRegistry.MarkQuitting<T>();
        }

        protected virtual void OnDestroy()
        {
            SingletonRegistry.Remove((T)this);
        }
    }
}
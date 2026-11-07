using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace PerfMonitor.Providers
{
    // Times the main thread's own work each frame by hooking the player loop.
    // It starts after Unity waits for the target frame rate and vsync, so idle time is not counted.
    // Works in release builds, where profiler markers return nothing.
    public static class MainThreadTimer
    {
        struct BeginWork { }
        struct EndWork { }

        static readonly Stopwatch Watch = new Stopwatch();
        static bool _installed;

        // Work time of the last finished frame in milliseconds, 0 until the first frame is timed
        public static float LastMs { get; private set; }

        public static void Install()
        {
            if (_installed)
            {
                return;
            }

            PlayerLoopSystem loop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                Type phase = loop.subSystemList[i].type;
                if (phase == typeof(EarlyUpdate))
                {
                    loop.subSystemList[i].subSystemList = Insert(loop.subSystemList[i].subSystemList, 0, typeof(BeginWork), Begin);
                }
                else if (phase == typeof(PostLateUpdate))
                {
                    PlayerLoopSystem[] systems = loop.subSystemList[i].subSystemList;
                    loop.subSystemList[i].subSystemList = Insert(systems, systems.Length, typeof(EndWork), End);
                }
            }

            PlayerLoop.SetPlayerLoop(loop);
            _installed = true;
        }

        // The player loop is rebuilt when play mode starts, so install again
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay()
        {
            _installed = false;
            LastMs = 0f;
        }

        static void Begin()
        {
            Watch.Restart();
        }

        static void End()
        {
            LastMs = (float)Watch.Elapsed.TotalMilliseconds;
        }

        static PlayerLoopSystem[] Insert(PlayerLoopSystem[] systems, int index, Type type, PlayerLoopSystem.UpdateFunction update)
        {
            var result = new PlayerLoopSystem[systems.Length + 1];
            Array.Copy(systems, 0, result, 0, index);
            result[index] = new PlayerLoopSystem { type = type, updateDelegate = update };
            Array.Copy(systems, index, result, index + 1, systems.Length - index);
            return result;
        }
    }
}

using System;
using MyTestTask.Abstraction;
using MyTestTask.Abstraction.Injection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MyTestTask
{
    [DefaultExecutionOrder(-10000)]
    public class Bootstrapper : MonoBehaviour
    {
        private DiContainer _diContainer;

        private void Awake()
        {
            _diContainer = new DiContainer();
            var scene = gameObject.scene;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var installer in root.GetComponentsInChildren<IInstaller>())
                {
                    installer.InstallBindings(_diContainer);
                }
            }
            
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var installer in root.GetComponentsInChildren<IInjectionTarget>())
                {
                    installer.Inject(_diContainer);
                }
            }
        }
    }
}
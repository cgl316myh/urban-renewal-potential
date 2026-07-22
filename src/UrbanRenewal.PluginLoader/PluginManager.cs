using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.PluginLoader
{
    /// <summary>
    /// 扫描 Plugins 目录，反射加载实现 IModulePlugin 的类型。
    /// </summary>
    public sealed class PluginManager
    {
        private readonly List<IModulePlugin> _plugins = new List<IModulePlugin>();
        private readonly Action<string> _logInfo;
        private readonly Action<string> _logError;

        public PluginManager(Action<string> logInfo, Action<string> logError)
        {
            _logInfo = logInfo ?? (delegate(string m) { });
            _logError = logError ?? (delegate(string m) { });
        }

        public IList<IModulePlugin> Plugins
        {
            get { return _plugins.AsReadOnly(); }
        }

        public void LoadAll(string pluginsDirectory)
        {
            _plugins.Clear();

            if (string.IsNullOrEmpty(pluginsDirectory) || !Directory.Exists(pluginsDirectory))
            {
                _logInfo("插件目录不存在，跳过加载: " + pluginsDirectory);
                return;
            }

            string[] files = Directory.GetFiles(pluginsDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                try
                {
                    LoadFromAssembly(file);
                }
                catch (Exception ex)
                {
                    _logError("加载插件失败 [" + Path.GetFileName(file) + "]: " + ex.Message);
                }
            }

            _plugins.Sort(delegate(IModulePlugin a, IModulePlugin b)
            {
                return a.Order.CompareTo(b.Order);
            });

            _logInfo("插件加载完成，共 " + _plugins.Count + " 个。");
        }

        public void InitializeAll(IAppContext context, IRibbonHost ribbonHost)
        {
            for (int i = 0; i < _plugins.Count; i++)
            {
                IModulePlugin plugin = _plugins[i];
                try
                {
                    plugin.Initialize(context);
                    plugin.RegisterRibbon(ribbonHost);
                    _logInfo("已初始化插件: " + plugin.Name + " (" + plugin.Id + ")");
                }
                catch (Exception ex)
                {
                    _logError("初始化插件失败 [" + plugin.Name + "]: " + ex.Message);
                }
            }
        }

        public void UnloadAll()
        {
            for (int i = _plugins.Count - 1; i >= 0; i--)
            {
                try
                {
                    _plugins[i].Shutdown();
                }
                catch (Exception ex)
                {
                    _logError("关闭插件失败: " + ex.Message);
                }
            }
            _plugins.Clear();
        }

        private void LoadFromAssembly(string assemblyPath)
        {
            Assembly asm = Assembly.LoadFrom(assemblyPath);
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
            }

            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (type == null || type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (!typeof(IModulePlugin).IsAssignableFrom(type))
                {
                    continue;
                }

                IModulePlugin plugin = (IModulePlugin)Activator.CreateInstance(type);
                _plugins.Add(plugin);
                _logInfo("发现插件: " + plugin.Name + " <- " + Path.GetFileName(assemblyPath));
            }
        }
    }
}

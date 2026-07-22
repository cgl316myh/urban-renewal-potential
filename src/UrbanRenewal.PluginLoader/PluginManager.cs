using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.PluginLoader
{
    /// <summary>
    /// 扫描 Plugins 目录，仅加载 UrbanRenewal.Plugins.*.dll。
    /// </summary>
    public sealed class PluginManager
    {
        private readonly List<IModulePlugin> _plugins = new List<IModulePlugin>();
        private readonly Action<string> _logInfo;
        private readonly Action<string> _logError;
        private static bool _resolveHooked;

        public PluginManager(Action<string> logInfo, Action<string> logError)
        {
            _logInfo = logInfo ?? (delegate(string m) { });
            _logError = logError ?? (delegate(string m) { });
            EnsureAssemblyResolve();
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

            _logInfo("扫描插件目录: " + pluginsDirectory);

            // 只加载插件程序集，避免把 Contracts/GIS 等依赖再 LoadFrom 造成类型不一致
            string[] files = Directory.GetFiles(pluginsDirectory, "UrbanRenewal.Plugins.*.dll", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            if (files.Length == 0)
            {
                _logInfo("未找到 UrbanRenewal.Plugins.*.dll，请先生成插件工程。");
                return;
            }

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
                    if (ex.InnerException != null)
                    {
                        _logError("  内部错误: " + ex.InnerException.Message);
                    }
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
                if (ex.LoaderExceptions != null)
                {
                    for (int e = 0; e < ex.LoaderExceptions.Length; e++)
                    {
                        if (ex.LoaderExceptions[e] != null)
                        {
                            _logError("反射类型失败: " + ex.LoaderExceptions[e].Message);
                        }
                    }
                }
            }

            Type pluginInterface = typeof(IModulePlugin);
            int found = 0;
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (type == null || type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (!pluginInterface.IsAssignableFrom(type))
                {
                    continue;
                }

                IModulePlugin plugin = (IModulePlugin)Activator.CreateInstance(type);
                _plugins.Add(plugin);
                found++;
                _logInfo("发现插件: " + plugin.Name + " <- " + Path.GetFileName(assemblyPath));
            }

            if (found == 0)
            {
                _logInfo("程序集中未发现 IModulePlugin: " + Path.GetFileName(assemblyPath));
            }
        }

        private static void EnsureAssemblyResolve()
        {
            if (_resolveHooked)
            {
                return;
            }
            _resolveHooked = true;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        /// <summary>
        /// 插件依赖优先从主程序目录加载，避免 Plugins 下重复 DLL 造成双份 Contracts。
        /// </summary>
        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                AssemblyName name = new AssemblyName(args.Name);
                string simple = name.Name;
                if (string.IsNullOrEmpty(simple))
                {
                    return null;
                }

                // 已加载则直接返回
                Assembly[] loaded = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < loaded.Length; i++)
                {
                    if (string.Equals(loaded[i].GetName().Name, simple, StringComparison.OrdinalIgnoreCase))
                    {
                        return loaded[i];
                    }
                }

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string candidate = Path.Combine(baseDir, simple + ".dll");
                if (File.Exists(candidate))
                {
                    return Assembly.LoadFrom(candidate);
                }
            }
            catch
            {
            }
            return null;
        }
    }
}

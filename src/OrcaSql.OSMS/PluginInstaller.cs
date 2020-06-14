using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ApplicationExtensibility.Plugin.Models;

namespace OrcaSql.OSMS
{
    internal class PluginInstaller
    {
        private readonly string _pluginDirectoryPath;
        private readonly Action<Func<IEnumerable<Stream>>> _openDatabaseFromStreams;
        private readonly IWin32Window _mainWindow;
#pragma warning disable 0649

        [ImportMany(typeof(IPlugin<string, IEnumerable<Stream>>))]
        private IEnumerable<IPlugin<string, IEnumerable<Stream>>> _plugins;

#pragma warning restore 0649

        private List<(ToolStripMenuItem parent, ToolStripItem child)> _installedMenus;

        public PluginInstaller(string pluginDirectoryPath, Action<Func<IEnumerable<Stream>>> openDatabaseFromStreams, IWin32Window mainWindow)
        {
            _pluginDirectoryPath = pluginDirectoryPath;
            _openDatabaseFromStreams = openDatabaseFromStreams;
            _mainWindow = mainWindow;
        }

        private static IEnumerable<string> GetAssemblyPaths(string pluginsPath)
        {
            if (!Directory.Exists(pluginsPath)) yield break;

            foreach (var fileName in Directory.EnumerateFiles(pluginsPath, "*.dll"))
            {
                string assemblyPath = null;
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(fileName);
                    if (assemblyName != null)
                    {
                        assemblyPath = fileName;
                    }
                }
                catch
                {
                    // ignored for non-managed assemblies
                }

                if (assemblyPath != null)
                {
                    yield return assemblyPath;
                }
            }
        }

        private static IEnumerable<Assembly> GetAssemblies(string pluginsPath)
        {
            var assemblyNames = GetAssemblyPaths(pluginsPath).ToArray();
            var assemblies = assemblyNames.Select(Assembly.LoadFrom);
            return assemblies;
        }

        public void ImportPlugins()
        {
            var mainModuleFileName = Process.GetCurrentProcess().MainModule?.FileName;

            if (mainModuleFileName == null) return;

            var appDirectoryName = Path.GetDirectoryName(mainModuleFileName) ?? string.Empty;

            var pluginsPath = Path.Combine(appDirectoryName, _pluginDirectoryPath);

            if (!Directory.Exists(pluginsPath)) return;

            _installedMenus = new List<(ToolStripMenuItem parent, ToolStripItem child)>();

            var catalog = new AggregateCatalog();

            foreach (var assembly in GetAssemblies(pluginsPath))
            {
                catalog.Catalogs.Add(new AssemblyCatalog(assembly));
            }

            var container = new CompositionContainer(catalog);

            container.ComposeParts(this);
        }

        public void InstallPlugins(MenuStrip menuStrip)
        {
            if (_plugins == null) return;

            foreach (var plugin in _plugins.Where(x => (x.PluginType & (PluginType.MenuItem | PluginType.ContainerReader)) != 0).OrderBy(x => x.MenuInfo.InsertIndex ?? int.MaxValue))
            {
                var parentMenu = menuStrip.Items.OfType<ToolStripMenuItem>().FirstOrDefault(x => x.Text == plugin.MenuInfo.ParentMenu);

                if (parentMenu == null) continue;

                var menuItem = new ToolStripMenuItem { Tag = plugin, Text = plugin.MenuInfo.MenuName };

                menuItem.Click += MenuItemOnClick;

                if (!string.IsNullOrEmpty(plugin.MenuInfo.ShortcutKeys))
                {
                    var enumKeys = plugin.MenuInfo
                        .ShortcutKeys
                        .Split('|')
                        .Select(x => Enum.TryParse<Keys>(x, out var key) ? key : Keys.None)
                        .Where(x => x != Keys.None)
                        .ToArray();

                    var keys = enumKeys.Aggregate(Keys.None, (current, key) => current | key);

                    if (keys != Keys.None)
                        menuItem.ShortcutKeys = keys;
                }

                if (plugin.MenuInfo.InsertIndex != null && parentMenu.DropDownItems.Count > plugin.MenuInfo.InsertIndex)
                {
                    parentMenu.DropDownItems.Insert(plugin.MenuInfo.InsertIndex.Value, menuItem);
                }
                else
                {
                    parentMenu.DropDownItems.Add(menuItem);
                }

                _installedMenus.Add((parentMenu, menuItem));
            }
        }

        public void UninstallPlugins()
        {
            if (!(_installedMenus?.Count > 0)) return;

            foreach (var (parent, child) in _installedMenus)
            {
                parent.DropDownItems.Remove(child);
                child.Click -= MenuItemOnClick;
                child.Dispose();
            }
        }

        private void MenuItemOnClick(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;

            if (!(menuItem?.Tag is IPlugin<string, IEnumerable<Stream>> plugin)) return;

            var dialog = new OpenFileDialog { Filter = plugin.MenuInfo.Filter, Multiselect = plugin.MenuInfo.MultiSelect };
            var result = dialog.ShowDialog(_mainWindow);

            if (result != DialogResult.OK) return;

            _openDatabaseFromStreams(() => plugin.GetData(dialog.FileName));
        }
    }
}

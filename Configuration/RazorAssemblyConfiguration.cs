using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace STZ.Frontend.Configuration;

public static class RazorAssemblyConfiguration
{
    public static Assembly[] GetAssembliesWithRazorPages(Assembly? exclude = null)
    {
        var mainAssembly = exclude ?? Assembly.GetEntryAssembly();

        // Cargar dinámicamente todos los ensamblados del directorio base
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetName().Name)
            .ToHashSet();

        var dlls = Directory.GetFiles(AppContext.BaseDirectory, "*.dll");
        foreach (var file in dlls)
        {
            try
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (!loadedAssemblies.Contains(name))
                    Assembly.Load(name);
            }
            catch { /* Ignorar errores de carga */ }
        }

        // Filtrar los ensamblados que contienen páginas con @page
        var result = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic && asm != mainAssembly)
            .Where(asm =>
            {
                try
                {
                    return asm.GetTypes().Any(t =>
                        typeof(ComponentBase).IsAssignableFrom(t) &&
                        t.GetCustomAttributes(typeof(RouteAttribute), inherit: true).Any());
                }
                catch
                {
                    return false;
                }
            })
            .ToArray();

        foreach (var asm in result)
        {
            Console.WriteLine($"[RazorAssemblyHelper] Ensamblado con páginas detectado: {asm.FullName}");
        }

        return result;
    }
}
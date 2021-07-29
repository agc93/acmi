using System;
using System.Linq;
using AceCore;
using BuildEngine.Builder;

namespace PackCreator.Build
{
    public static class BuildInstructionFactory
    {
        public static BuildInstruction? GetInstructionForIdentifier(Identifier ident) {
            try {
                var instructionType = typeof(BuildInstruction<>);
                var typeArgs = ident.GetType();
                var constructed = instructionType.MakeGenericType(typeArgs);
                var assembly = typeof(Startup).Assembly
                    .GetTypes().FirstOrDefault(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(constructed));
                if (assembly != null) {
                    return Activator.CreateInstance(assembly, ident) as BuildInstruction;
                }
            }
            catch {
                return null;
            }
            return null;
        }
    }
}
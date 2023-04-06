using Autofac.Core;
using Autofac;
using RoslynCat.Interface;
using Microsoft.CodeAnalysis.Completion;
using RoslynCat.Roslyn;

namespace RoslynCat.Controllers
{
    public class Test
    {
    }

    public class MyApplicationModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<Roslyn.CompletionProvider>().As<ICompleteProvider>();
            builder.RegisterType<HoverProvider>().As<IHoverProvider>();
            builder.RegisterType<CodeCheckProvider>().As<ICodeCheckProvider>();
            builder.RegisterType<CompletionDocument>().SingleInstance();
        }
    }
}

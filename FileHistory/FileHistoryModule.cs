using Autofac;

namespace J4JSoftware.FileHistory
{
    public class FileHistoryModule : Module
    {
        protected override void Load( ContainerBuilder builder )
        {
            base.Load( builder );

            builder.RegisterType<FileHistoryConfiguration>()
                .As<IFileHistoryConfiguration>()
                .SingleInstance();

            builder.RegisterType<FileHistoryService>()
                .As<IFileHistoryService>()
                .SingleInstance();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autofac;
using Newtonsoft.Json;

namespace J4JSoftware.FileHistory
{
    public class FileHistoryShareTargetModule : Module
    {
        protected override void Load( ContainerBuilder builder )
        {
            base.Load( builder );

            builder.RegisterType<FileHistoryShareTarget>()
                .As<IFileHistoryTarget>();
        }
    }
}

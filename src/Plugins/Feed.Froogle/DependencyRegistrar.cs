using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data;
using SmartStore.Plugin.Feed.Froogle.Data;
using SmartStore.Plugin.Feed.Froogle.Domain;
using SmartStore.Plugin.Feed.Froogle.Services;

namespace SmartStore.Plugin.Feed.Froogle
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            builder.RegisterType<GoogleFeedService>().As<IGoogleFeedService>().InstancePerRequest();

            //register named context
			builder.Register<IDbContext>(c => new GoogleProductObjectContext(DataSettings.Current.DataConnectionString))
                .Named<IDbContext>(GoogleProductObjectContext.ALIASKEY)
                .InstancePerRequest();

			builder.Register<GoogleProductObjectContext>(c => new GoogleProductObjectContext(DataSettings.Current.DataConnectionString))
                .InstancePerRequest();

            //override required repository with our custom context
            builder.RegisterType<EfRepository<GoogleProductRecord>>()
                .As<IRepository<GoogleProductRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(GoogleProductObjectContext.ALIASKEY))
                .InstancePerRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}

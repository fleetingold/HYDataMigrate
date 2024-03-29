﻿using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Hangfire.Topshelf.Jobs;
using Hangfire.Samples.Framework;
using Hangfire.Samples.Framework.Logging;
using SqlSugar;
using Models;

namespace Hangfire.Topshelf.Core
{
	/// <summary>
	/// Hangfire Module
	/// </summary>
	public class HangfireModule : Autofac.Module
	{
		protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
		{
			base.AttachToComponentRegistration(componentRegistry, registration);

			// Handle constructor parameters.
			registration.Preparing += OnComponentPreparing;

			// Handle properties.
			registration.Activated += (sender, e) => InjectLoggerProperties(e.Instance);
		}

		private void InjectLoggerProperties(object instance)
		{
			var instanceType = instance.GetType();

			// Get all the injectable properties to set.
			// If you wanted to ensure the properties were only UNSET properties,
			// here's where you'd do it.
			var properties = instanceType
			  .GetProperties(BindingFlags.Public | BindingFlags.Instance)
			  .Where(p => p.PropertyType == typeof(ILog) && p.CanWrite && p.GetIndexParameters().Length == 0);

			// Set the properties located.
			foreach (var propToSet in properties)
			{
				propToSet.SetValue(instance, LogProvider.GetLogger(instanceType), null);
			}
		}

		private void OnComponentPreparing(object sender, PreparingEventArgs e)
		{
			e.Parameters = e.Parameters.Union(new[]
				 {
					new ResolvedParameter(
						(p, i) => p.ParameterType == typeof(ILog),
						(p, i) => LogProvider.GetLogger(p.Member.DeclaringType)
					),
				 });
		}

		/// <summary>
		/// Auto register
		/// </summary>
		/// <param name="builder"></param>
		protected override void Load(ContainerBuilder builder)
		{
			//register all implemented interfaces
			builder.RegisterAssemblyTypes(ThisAssembly)
				.Where(t => typeof(IDependency).IsAssignableFrom(t) && t != typeof(IDependency) && !t.IsInterface)
				.AsImplementedInterfaces();

            //register speicified types here
            builder.Register(x => new RecurringJobService() {
                SourceDB = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = HangfireSettings.Instance.SqlSugarSqlserverHYERPBusiness10ConnectionString, //必填
                    DbType = DbType.SqlServer, //必填
                    IsAutoCloseConnection = true
                }),
                TargetDB = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = HangfireSettings.Instance.SqlSugarSqlserverHYERPBusinessConnectionString, //必填
                    DbType = DbType.SqlServer, //必填
                    IsAutoCloseConnection = true
                })
            }).SingleInstance();
            builder.Register(x => new MyJob1());
            builder.Register(x => new MyJob2());
            builder.Register(x => new LongRunningJob());
            builder.RegisterType<Base_AreaList>();

            // createModel
            //CreateModel();
        }

        private void CreateModel() {
            SqlSugarClient TargetSqlSugarClient = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = HangfireSettings.Instance.SqlSugarSqlserverHYERPBusinessConnectionString, //必填
                DbType = DbType.SqlServer, //必填
                IsAutoCloseConnection = true
            });

            //TargetSqlSugarClient.DbFirst.Where("AreaList").CreateClassFile(@"C:\Users\HYKJ8\source\repos\HYDataMigrate\Hangfire.Topshelf\DataModel");
            TargetSqlSugarClient.DbFirst.Where("FinaSettlement").CreateClassFile(@"C:\Users\HYKJ8\source\repos\HYDataMigrate\Hangfire.Topshelf\DataModel");

            //SqlSugarClient SourceSqlSugarClient = new SqlSugarClient(new ConnectionConfig()
            //{
            //    ConnectionString = HangfireSettings.Instance.SqlSugarSqlserverHYERPBusiness10ConnectionString, //必填
            //    DbType = DbType.SqlServer, //必填
            //    IsAutoCloseConnection = true
            //});

            //SourceSqlSugarClient.DbFirst.Where("FinaSettlement").CreateClassFile(@"C:\Users\HYKJ8\source\repos\HYDataMigrate\Hangfire.Topshelf\DataModel");
        }
    }
}

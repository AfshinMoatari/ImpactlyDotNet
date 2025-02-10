using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dynamo.Seeds;
using API.Dynamo.Seeds.Surveys;
using API.Handlers;
using API.Models;
using API.Models.Admin;
using API.Models.Config;
using API.Models.Projects;
using API.Models.Strategy;
using API.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace API.Dynamo
{
    public static class DynamoSeedHelper
    {
        public static async Task SeedTables(IServiceProvider serviceProvider)
        {
            try
            {
                serviceProvider.GetRequiredService<JWTConfig>();

                var projectContext = serviceProvider.GetRequiredService<IProjectContext>();
                var adminContext = serviceProvider.GetRequiredService<IAdminContext>();
                var strategyContext = serviceProvider.GetRequiredService<IStrategyContext>();
                var userContext = serviceProvider.GetRequiredService<IUserContext>();
                var patientContext = serviceProvider.GetRequiredService<IPatientContext>();
                var roleContext = serviceProvider.GetRequiredService<IRoleContext>();
                var notificationContext = serviceProvider.GetRequiredService<INotificationContext>();
                // Testing
                //var cronContext = serviceProvider.GetRequiredService<ICronContext>();
                // var analyticsContext = serviceProvider.GetRequiredService<IAnalyticsContext>();

                var authHandler = serviceProvider.GetRequiredService<IAuthHandler>();

                await Seed(projectContext.Projects, DynamoSeedAdmin.Project);
                await SeedAll(roleContext.Roles, RoleSeeds.Roles);

                // seed admin
                var admin = DynamoSeedAdmin.AdminUser;
                admin.PasswordHashB64 = authHandler.HashUserPassword(admin.Id, DynamoSeedAdmin.AdminPassword);
                await userContext.Users.ReadOrCreate(admin);
                await projectContext.CreateProjectUser(DynamoSeedAdmin.Project.Id, new ProjectUser
                {
                    Id = admin.Id,
                    Email = admin.Email,
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    PhoneNumber = admin.PhoneNumber,
                    RoleId = RoleSeeds.SuperRole.Id,
                });

                await adminContext.Admins.Create(AdminUser.FromAuthUser(admin));
                await SeedAll(strategyContext.Surveys, TemplateSurveySeeds.Surveys);
                await SeedAll(strategyContext.SurveyFields, TemplateSurveySeeds.SurveyFields);
                await SeedAll(strategyContext.FieldChoices, TemplateSurveySeeds.GenerateSurveyChoices());

                await SeedAll(patientContext.ProjectPatients, PatientSeeds.All);
                await Seed(strategyContext.Strategies, StrategySeeds.StrategyOne);
                // await Seed(cronContext.SurveyJobs, StrategySeeds.SurveyJob);
                await Seed(strategyContext.StrategyPatients, StrategySeeds.StrategyPatient);
                await Seed(strategyContext.BatchFrequencies, StrategySeeds.StrategyOneFrequency);
                await SeedAll(strategyContext.Effects, StrategySeeds.Effects);
                await SeedAll(notificationContext.Notifications, NotificationSeed.All);
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Failed to seed tables");
                throw;
            }
        }

        private static async Task Seed<T>(ICrudPropertyRepository<T> repository, T seed) where T : class, ICrudPropModel
        {
            var typeName = typeof(T).Name;
            var existing = await repository.Read(seed.ParentId, seed.Id);
            if (existing == null)
            {
                Console.WriteLine($"{typeName}: {seed.Id} SEEDING");
                await repository.Create(seed.ParentId, seed);
            }
        }

        private static async Task Seed<T>(ICrudRepository<T> repository, T seed) where T : class, ICrudPropModel
        {
            var typeName = typeof(T).Name;
            var existing = await repository.Read(seed.Id);
            if (existing == null)
            {
                Console.WriteLine($"{typeName}: {seed.Id} SEEDING");
                await repository.Create(seed);
            }
        }

        private static async Task SeedAll<T>(ICrudRepository<T> repository, IEnumerable<T> seedList)
            where T : class, ICrudPropModel
        {
            var typeName = typeof(T).Name;
            var existing = (await repository.ReadAll())?.ToList() ?? new List<T>();
            await Task.WhenAll(seedList.Select(async seed =>
            {
                if (existing.All(e => e.Id != seed.Id))
                {
                    Console.WriteLine($"{typeName}: {seed.Id} SEEDING");
                    await repository.Create(seed);
                }
            }));
        }

        private static async Task SeedAll<T>(ICrudPropertyRepository<T> repository, List<T> seedList)
            where T : class, ICrudPropModel
        {
            var typeName = typeof(T).Name;

            foreach (var seed in seedList)
            {
                var existing = (await repository.ReadAll(seed.ParentId))?.ToList() ?? new List<T>();

                if (existing.All(e => e.Id != seed.Id))
                {
                    Console.WriteLine($"{typeName}: {seed.Id} SEEDING");
                    await repository.Create(seed.ParentId, seed);
                }
            }
        }
    }
}
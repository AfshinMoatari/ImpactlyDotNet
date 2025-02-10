using System;
using System.Collections.Generic;
using API.Models.Projects;

namespace API.Dynamo.Seeds
{
    public class PatientSeeds
    {
        public static readonly List<ProjectPatient> All = new List<ProjectPatient>
        {
            new ProjectPatient
            {
                Id = "patient1",
                FirstName = "Christian",
                LastName = "Poulsen",
                Email = "christian+test1@innosocial.dk",
                Sex = "Han",
                BirthDate = new DateTime(1998, 5, 25),
                ParentId = DynamoSeedAdmin.Project.Id,
                PostalNumber = "2800",
                Municipality = "Lyngby-Taarbæk",
                Region = "Hovedstaden",
                IsActive = true,
                StrategyId = StrategySeeds.StrategyOne.Id,
                StrategyName = StrategySeeds.StrategyOne.Name
            },
            new ProjectPatient
            {
                Id = "patient4",
                FirstName = "Emil",
                LastName = "Georgi",
                Email = "emil@impactly.dk",
                Sex = "Han",
                BirthDate = new DateTime(1998, 11, 27),
                ParentId = DynamoSeedAdmin.Project.Id,
                PostalNumber = "2670",
                Municipality = "Greve",
                Region = "Sjælland",
                IsActive = true,
                StrategyId = StrategySeeds.StrategyOne.Id,
                StrategyName = StrategySeeds.StrategyOne.Name
            },
            new ProjectPatient
            {
                Id = "patient2",
                FirstName = "Nicolai",
                LastName = "Lassen",
                Email = "christian+test2@innosocial.dk",
                Sex = "Han",
                BirthDate = new DateTime(1996, 11, 23),
                ParentId = DynamoSeedAdmin.Project.Id,
                PostalNumber = "2100",
                Municipality = "København",
                Region = "Hovedstaden",
                IsActive = true,
            },
            new ProjectPatient
            {
                Id = "patient3",
                FirstName = "Freja",
                LastName = "Boysen",
                Email = "christian+test3@innosocial.dk",
                Sex = "Hun",
                BirthDate = new DateTime(1995, 1, 2),
                ParentId = DynamoSeedAdmin.Project.Id,
                PostalNumber = "2500",
                Municipality = "København",
                Region = "Hovedstaden",
                IsActive = true,
            },
        };
    }
}
using System;

namespace API.Constants
{
    public struct Permissions
    {
        public struct Admin
        {
            public const string All = "Permissions.Admin.All";
        }

        public struct Users
        {
            public const string Read = "Permissions.Users.Read";
            public const string Write = "Permissions.Users.Write";
        }

        public struct Auth
        {
            public const string CreatePassword = "Permissions.Create.Password";
            public const string UpdatePassword = "Permissions.Update.Password";
        }

        public struct Strategy
        {
            public const string Read = "Permissions.Strategy.Read";
            public const string Write = "Permissions.Strategy.Write";
        }

        public struct Project
        {
            public const string Read = "Permissions.Settings.Read";
            public const string Write = "Permissions.Settings.Write";
        }
    }
}
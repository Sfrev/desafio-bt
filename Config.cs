﻿namespace desafio_bt
{
    public class Config(string token, string destinationEmail, string host, int port, string username, string password)
    {
        private readonly string token = token;
        private readonly string destinationEmail = destinationEmail;
        private readonly string host = host;
        private readonly int port = port;
        private readonly string username = username;
        private readonly string password = password;

        public String GetToken()
        {
            return token;
        }
        public String GetDestinationEmail()
        {
            return destinationEmail;
        }
        public String GetHost()
        {
            return host;
        }
        public int GetPort()
        {
            return port;
        }
        public string GetUsername()
        {
            return username;
        }
        public string GetPassword()
        {
            return password;
        }
    }
}
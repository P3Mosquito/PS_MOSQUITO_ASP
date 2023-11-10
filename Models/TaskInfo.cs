﻿namespace ps_mosquito_asp.Models
{
    public class TaskInfo
    {
        public string Id { get; set; }
        public string Zona { get; set; }
        public string Estado { get; set; }
        public List<Dictionary<string, double>> Coordenadas { get; set; }
    }

}

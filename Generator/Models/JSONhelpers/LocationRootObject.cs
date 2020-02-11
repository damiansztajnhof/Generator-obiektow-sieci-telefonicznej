﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Generator.Models
{
    public class Location1
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    public class Geometry
    {
        public Location1 location { get; set; }
        
    }
    public class Result
    {
        public Geometry geometry { get; set; }
     
        
        public string name { get; set; }
       
        public string place_id { get; set; }
   
    }
    public class LocationRootObject
    {
       
        public List<Result> results { get; set; }

        
    }
}
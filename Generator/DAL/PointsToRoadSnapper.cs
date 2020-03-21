﻿using Generator.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web;

namespace Generator.DAL
{
    public class PointsToRoadSnapper
    {
        public AllHumansPositions SnapPointsToRoad(GeneratorContext context)
        {
            AllHumansPositions fullDrawTable = new AllHumansPositions();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var humansToDraw = context.People.Include(x => x.HumanRoutes.Select(l => l.legs.Select(s => s.steps.Select(d=>d.duration)))).Include
                (x => x.HumanRoutes.Select(l => l.legs.Select(s => s.steps.Select(d => d.end_location)))).Include
                (x => x.HumanRoutes.Select(l => l.legs.Select(s => s.steps.Select(d => d.start_location)))).ToList();
            var humantypes = context.humanTypes;
            foreach (var h in humansToDraw)
            {
                string humanType = humantypes.Where(g => h.HumanTypeId == g.HumanTypeId).FirstOrDefault().HumanTypeName;
                string color = humantypes.Where(g => h.HumanTypeId == g.HumanTypeId).FirstOrDefault().color;
                Root root = new Root();
                int iterator = 0;
                string url = $"https://roads.googleapis.com/v1/snapToRoads?path=";
                HumanPositions humanPositions = new HumanPositions();
                
               

                
                double beforelat = 0;
                double beforelng = 0;
                humanPositions.pozycje.Add(new SingleHumanPosition { Lat = h.HumanRoutes.First().legs.First().steps.First().start_location.lat, Lng = h.HumanRoutes.First().legs.First().steps.First().start_location.lng });
                if (h.LocomotionType == "&mode=walking")
                {

                    foreach (var i in h.HumanRoutes)
                    {

                        foreach (var s in i.legs.First().steps)
                        {
                            humanPositions.pozycje.Add(new SingleHumanPosition { Lat = s.end_location.lat, Lng = s.end_location.lng });
                        }

                    }
                    fullDrawTable.fullTable.Add(new HumanPositions { pozycje = humanPositions.pozycje, humantype = humanType, color = color });
                }
                else
                {
                    foreach (var i in h.HumanRoutes)
                    {
                        foreach (var s in i.legs.First().steps)
                        {
                            double midpointlat = (beforelat + s.start_location.lat) / 2;
                            double midpointlng = (beforelng + s.start_location.lng) / 2;
                            if (iterator == 0)
                            {
                                url = url + s.start_location.lat.ToString().Replace(",", ".") + "," + s.start_location.lng.ToString().Replace(",", ".");
                            }
                            else
                            {
                                url += "|" + midpointlat.ToString().Replace(",", ".") + "," + midpointlng.ToString().Replace(",", ".");
                                url = url + "|" + s.start_location.lat.ToString().Replace(",", ".") + "," + s.start_location.lng.ToString().Replace(",", ".");
                            }
                            beforelat = s.start_location.lat;
                            beforelng = s.start_location.lng;
                            iterator++;
                        }

                    }
                    url = url + "&interpolate=true&key=AIzaSyA5jOPVeNOqU6lLscGSE3t665ejKNjrGQI";
                    var response = client.GetAsync(url).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;
                        root = responseContent.ReadAsAsync<Root>().GetAwaiter().GetResult();
                        foreach (var i in root.snappedPoints)
                        {
                            humanPositions.pozycje.Add(new SingleHumanPosition { Lat = i.location.latitude, Lng = i.location.longitude });
                        }

                        fullDrawTable.fullTable.Add(new HumanPositions { pozycje = humanPositions.pozycje, humantype = humanType, color = color });
                    }
                    else
                    {
                    }
                }
            }
            return fullDrawTable;
        }
    }
}
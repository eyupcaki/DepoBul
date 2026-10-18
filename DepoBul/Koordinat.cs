using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace DepoBul
{
  public  class Koordinat
    {
       

        public static double UzaklikHesapla(double en1, double boy1, double en2, double boy2)
        {


            double R = 6371000; // m  Dünyanın yarıçapı
            double dEn= (en2 - en1) / 180.0 * Math.PI;

            double dBoy = (boy2 - boy1) / 180.0 * Math.PI;

            double a = Math.Sin(dEn / 2.0) * Math.Sin(dEn / 2.0) +
                    Math.Sin(dBoy / 2.0) * Math.Sin(dBoy / 2.0) * Math.Cos(en1 / 180.0 * Math.PI) * Math.Cos(en2 / 180.0 * Math.PI);
            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            return R * c;
        }


        public static double ikiNoktaUzaklik2(double en1, double boy1, double en2, double boy2)
        {
            bool rotahesap = false;
           
            PointLatLng baslagic;
            PointLatLng bitis;
            double mesafe = 0;

            rotahesap = true;
            if (rotahesap)
            {
                baslagic = new PointLatLng(en1, boy1);
                bitis = new PointLatLng(en2, boy2);
                GDirections direccion;

                var RutasDireccion = GMapProviders.GoogleMap.GetDirections(out direccion, baslagic, bitis, false, false, false, false, false);
                GMapRoute RutaObtenida = new GMapRoute(direccion.Route, "Rota Konumu");
                GMapOverlay Caparutas = new GMapOverlay("Rota Mesafe");
                Caparutas.Routes.Add(RutaObtenida);
                //gMapControl1.Overlays.Add(Caparutas);
                //gMapControl1.Zoom = gMapControl1.Zoom + 1;
                //gMapControl1.Zoom = gMapControl1.Zoom - 1;
                rotahesap = false;

                mesafe = RutaObtenida.Distance;
            }

            return Math.Abs(mesafe);
        }

        public static string SwapCharacters(string value, int position1, int position2)
        {
            // eski kromozomları  yeni kromozomlar ile değiştir. 

            char[] array = value.ToCharArray(); 
            char temp = array[position1]; 
            array[position1] = array[position2]; 
            array[position2] = temp;
            return new string(array); 
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace EventoMercantiles.Datos
{
    public class CadenaConexion
    {
        public static List<string> ReadConexion()
        {
           
            List<string> con = new();
            StreamReader sr = new(Environment.CurrentDirectory + "/env.txt");
            con.Add(sr.ReadLine());
            con.Add(sr.ReadLine());
            con.Add(sr.ReadLine());
            sr.Close();
            return con;
        }

        public static void WriteConexion(string ip, string baseda, string codigo)
        {
            //Pass the filepath and filename to the StreamWriter Constructor
            StreamWriter sw = new(Environment.CurrentDirectory + "/env.txt");
            //Write a line of text
            sw.WriteLine(ip);
            sw.WriteLine(baseda);
            sw.WriteLine(codigo);
            //Close the file
            sw.Close();
        }

    }
}

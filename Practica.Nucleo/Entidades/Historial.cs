﻿using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Practica.Nucleo.Enumeradores;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Practica.Nucleo.Entidades
{
    public class Historial : Persistent
    {
        public override int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string Ciudad { get; set; }
        public Estado EstadoPaquete { get; set; }
        public Usuario Usuario { get; set; }
        

        public static IList<HistorialDTO> ObtenerTodos()
        {
            IList<Historial> historiales;
            IList<HistorialDTO> historialesTransporte = new List<HistorialDTO>();
            try
            {
                using (ISession session = Persistent.SessionFactory.OpenSession())
                {
                    ICriteria crit = session.CreateCriteria(new Historial().GetType());
                    historiales = crit.List<Historial>();


                    for (int i = 0; i < historiales.Count; i++)
                    {
                        HistorialDTO hdt = new HistorialDTO();
                        hdt.Id = historiales[i].Id;
                        hdt.Fecha = historiales[i].Fecha.ToString("MM/dd/yyyy");
                        hdt.Ciudad = historiales[i].Ciudad;
                        hdt.Estado = historiales[i].Estado;
                        hdt.Descripcion = historiales[i].Descripcion;
                        int estado = (int)historiales[i].EstadoPaquete;

                        if (estado == 1)
                        {
                            hdt.EstadoPaquete = "PENDIENTE";
                        }
                        else if (estado == 2)
                        {
                            hdt.EstadoPaquete = "ENTREGADO";
                        }
                        else
                        {
                            hdt.EstadoPaquete = "CANCELADO";
                        }
                        
                        historialesTransporte.Add(hdt);
                    }

                    session.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return historialesTransporte;
        }

        public static IList<HistorialDTO> ObtenerPorOrden(String id)
        {
            IList<Historial> historiales;
            IList<HistorialDTO> historialTransporte = new List<HistorialDTO>(); ;
            try
            {
                Orden o = Orden.ObtenerPorFolio(id);
                historiales = o.Historiales;
                for (int i = 0; i < historiales.Count; i++)
                {
                    HistorialDTO hdt = new HistorialDTO();
                    hdt.Id = historiales[i].Id;
                    hdt.Fecha = historiales[i].Fecha.ToString("MM/dd/yyyy");
                    hdt.Descripcion = historiales[i].Descripcion;
                    hdt.Estado = historiales[i].Estado;
                    hdt.Ciudad = historiales[i].Ciudad;
                    int estado = (int)historiales[i].EstadoPaquete;

                    if (estado == 1)
                    {
                        hdt.EstadoPaquete = "PENDIENTE";
                    }
                    else if (estado == 2)
                    {
                        hdt.EstadoPaquete = "ENTREGADO";
                    }
                    else
                    {
                        hdt.EstadoPaquete = "CANCELADO";
                    }

                    historialTransporte.Add(hdt);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return historialTransporte;
        }

        public static Historial ObtenerPorId(int id)
        {
            Historial h = new Historial();
            try
            {
                using (ISession session = Persistent.SessionFactory.OpenSession())
                {
                    ICriteria crit = session.CreateCriteria(h.GetType());
                    crit.Add(Expression.Eq("Id", id));
                    h = (crit.UniqueResult<Historial>());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return h;
        }

        public static bool Guardar(int id, string descripcion, string ciudad, string estado,int estadoPaquete, int idUsuario, String idOrden)
        {
            bool realizado = false;
            try
            {

                Usuario u = Usuario.ObtenerPorId(idUsuario);
                Historial h = id == 0 ? new Historial() : ObtenerPorId(id);
                h.Fecha = DateTime.Now;
                h.Descripcion = descripcion;
                h.Estado = estado;
                h.Ciudad = ciudad;
                h.EstadoPaquete = (Estado) estadoPaquete;
                h.Usuario = u;
                if (id != 0)
                {
                    h.Update();
                }
                else
                {
                    Orden o = Orden.ObtenerPorFolio(idOrden);
                    IList<Historial> historiales = o.Historiales;
                    historiales.Add(h);
                    o.Historiales = historiales;
                    o.Update();
                }
                EnviarEmail(h, idOrden);
                realizado = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return realizado;
        }

        public static bool Borrar(int id)
        {
            bool realizado = false;
            try
            {
                Historial h = ObtenerPorId(id);
                h.Delete();

                realizado = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return realizado;
        }


        public static void EnviarEmail(Historial h, string idOrden)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential("track.paack@gmail.com", "tp2018**");
                MailMessage mmsg = new MailMessage();

                Orden o = Orden.ObtenerPorFolio(idOrden);
                int idCliente = o.Cliente.Id;
                int idDestinatario = o.Destinatario.Id;
                Cliente c = Cliente.ObtenerPorId(idCliente);
                string correoCliente = c.Correo;
                Destinatario d = Destinatario.ObtenerPorId(idDestinatario);
                string correoDestinatario = d.Correo;


                mmsg.To.Add(correoCliente);
                mmsg.To.Add(correoDestinatario);
                mmsg.Subject = "La localizacion de tu paquete se ha actualizado.";
                mmsg.SubjectEncoding = Encoding.UTF8;

                body = body.Replace("[NOMBRECLIENTE]", c.Nombre).Replace("[NOMBREDESTINATARIO]", d.Nombre)
                    .Replace("[FECHAORDEN]", o.Fecha.ToString("dd/MM/YYYY"))
                    .Replace("[FECHAORDEN]", o.Fecha.ToString("dd/MM/YYYY"))
                    .Replace("[PRECIOORDEN]", o.Precio.ToString())
                    .Replace("[NUMRASTREOORDEN]", o.NumeroRastreo)
                    .Replace("[ESTADOORDEN]", o.Estado.ToString())
                    .Replace("[FECHAHISTORIAL]", h.Fecha.ToString("dd/MM/YYYY"))
                    .Replace("[FECHAHISTORIAL]", h.Fecha.ToString("dd/MM/YYYY"))
                    .Replace("[DESCRIPCIONHISTORIAL]", h.Descripcion)
                    .Replace("[ESTADOHISTORIAL]", h.Estado)
                    .Replace("[CIUDADHISTORIAL]", h.Ciudad)
                    .Replace("[EPHISTORIAL]", h.EstadoPaquete.ToString());


                mmsg.Body = body;
                mmsg.BodyEncoding = Encoding.UTF8;
                mmsg.IsBodyHtml = true;
                mmsg.From = new MailAddress("track.paack@gmail.com");
                client.Send(mmsg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        static string body = "<html>" +
                           "<head>" +
                           "<title>Historial de paquete.</title>" +
                           "</head>" +
                           "<body>" +
                           "Hola [NOMBRECLIENTE], tu orden ha sido actualizada." +
                           "Con los siguientes datos: " +
                           "Fecha: [FECHAORDEN] Número de rastreo: [NUMRASTREOORDEN] Precio: [PRECIOORDEN] Estado de la orden: [ESTADOORDEN]." +
                           "Remitente: " +
                           "Nombre: [NOMBRECLIENTE] " +
                           "Destinatario: " +
                           "Nombre: [NOMBREDESTINATARIO] " +
                           "Historial: " +
                           "Fecha: [FECHAHISTORIAL] Descripcion: [DESCRIPCIONHISTORIAL] Estado: [ESTADOHISTORIAL] Ciudad: [CIUDADHISTORIAL] Ciudad: [CIUDADHISTORIAL] Estado de paquete [EPHISTORIAL]" +
                           "</body>" +
                           "</html>";
    }



}

﻿using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Practica.Nucleo.Enumeradores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Practica.Nucleo.Entidades
{
    public class Orden : Persistent
    {
        public override int Id { get; set; }
        public string Folio { get; set; }
        public DateTime Fecha { get; set; }
        public Cliente Cliente { get; set; }
        public Destinatario Destinatario { get; set; }
        public Usuario Usuario { get; set; }
        public Paquete Paquete { get; set; }
        public IList<Historial> Historiales { get; set; }
        public double Precio { get; set; }
        public string NumeroRastreo { get; set; }
        public Estado Estado { get; set; }

        public static string ObtenerFolio()
        {
            DateTime date = DateTime.Now;
            string folio ="";
            string idu = "";
            int n;
            string año = Convert.ToString(date.Year);

            using (ISession session = Persistent.SessionFactory.OpenSession())
            {
                var id = session.CreateSQLQuery("Select max(folio) from trackpackdb.orden;")
                            .UniqueResult();
                if (id != null) {
                    idu = Convert.ToString(id);
                    idu = idu.Remove(0, 4);
                }
                else
                {
                    idu = "0";
                }


            }
            
            

            n = Convert.ToInt32(idu) +1;
            string ceros;

            
            if (n > 999999)
            {                //001999
                if (n != 0)
                {
                    n = 1;
                }
                ceros = "00000";
                string letra = "A";
                folio = letra + año + ceros + n;
            }
            if (n > 99999)
            {
                ceros = "";
                folio = año + ceros + n;
            }
            if (n > 9999)
            {
                ceros = "0";
                folio = año + ceros + n;
            }
            if (n > 999)
            {
                ceros = "00";
                folio = año + ceros + n;
            }
            if (n > 99)
            {
                ceros = "000";
                folio = año + ceros + n;
            }
            if (n > 9)
            {
                ceros = "0000";
                folio = año + ceros + n;
            }
            if (n <= 9)
            {
                ceros = "00000";
                folio = año + ceros + n;
            }

            return folio;
        }

        public static IList<OrdenDTO> ObtenerTodos()
        {
            IList<Orden> ordenes;
            IList<OrdenDTO> ordenesTransporte = new List<OrdenDTO>();
            try
            {
                using (ISession session = Persistent.SessionFactory.OpenSession())
                {
                    ICriteria crit = session.CreateCriteria(new Orden().GetType());
                    ordenes = crit.List<Orden>();


                    for(int i = 0; i<ordenes.Count; i++)
                    {
                        OrdenDTO odt = new OrdenDTO();
                        odt.Id = ordenes[i].Id;
                        odt.Folio = ordenes[i].Folio;
                        odt.NumeroRastreo = ordenes[i].NumeroRastreo;
                        int estado = (int)ordenes[i].Estado;
                        if(estado == 1)
                        {
                            odt.Estado = "PENDIENTE";
                        }else if (estado== 2)
                        {
                            odt.Estado = "ENTREGADO";
                        }
                        else
                        {
                            odt.Estado = "CANCELADO";
                        }

                        odt.Fecha = ordenes[i].Fecha.ToString("MM/dd/yyyy");
                        ordenesTransporte.Add(odt);
                    }
                    
                    session.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ordenesTransporte;
        }

        public static OrdenDTO ObtenerDatosOrden()
        {
            OrdenDTO o = new OrdenDTO();
            o.Folio = ObtenerFolio();
            o.Fecha = DateTime.Now.ToString("MM/dd/yyyy");
            o.NumeroRastreo = o.Folio;
            o.Estado = Estado.PENDIENTE.ToString();
            return o;
        }
        public static Orden ObtenerPorId(int id)
        {
            Orden o = new Orden();
            try
            {
                using (ISession session = Persistent.SessionFactory.OpenSession())
                {
                    ICriteria crit = session.CreateCriteria(o.GetType());
                    crit.Add(Expression.Eq("Id", id));
                    o = (crit.UniqueResult<Orden>());
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return o;
        }
        public static bool Guardar(int idOrden, int ordenEstado, double ordenPrecio, string ordenFolio, string ordenNumRastreo, string ordenFecha,
                                    int idUsuario,
                                    int idPaquete, string paquetePeso, string paqueteTamanio, string paqueteContenido, string paqueteDescripcion,
                                    int idCliente, string clienteNombre, string clienteTelefono, string clienteCorreo, string clienteRfc, string clienteDomicilio,
                                    int idDestinatario, string destinatarioNombre, string destinatarioTelefono, string destinatarioCorreo, string destinatarioPersona,
                                    string destinatarioCalle, string destinatarioNumero, string destinatarioAvenida, string destinatarioColonia, string destinatarioCp,
                                    string destinatarioCiudad, string destinatarioEstado, string destinatarioReferencia)
        {
            bool realizado = false;
            try
            {

                Usuario u = Usuario.ObtenerPorId(idUsuario);

                Paquete p = idPaquete == 0 ? new Paquete() : Paquete.ObtenerPorId(idPaquete);
                p.Peso = paquetePeso;
                p.Tamanio = paqueteTamanio;
                p.Contenido = paqueteContenido;
                p.Descripcion = paqueteDescripcion;
                if (idPaquete != 0) { p.Update(); } else { p.Save(); }

                Cliente c = idCliente == 0 ? new Cliente() : Cliente.ObtenerPorId(idCliente);
                c.Nombre = clienteNombre;
                c.Domicilio = clienteDomicilio;
                c.Telefono = clienteTelefono;
                c.Correo = clienteCorreo;
                c.Rfc = clienteRfc;
                if (idCliente != 0) { c.Update(); } else { c.Save(); }

                Destinatario d = idDestinatario == 0 ? new Destinatario() : Destinatario.ObtenerPorId(idDestinatario);
                d.Nombre = destinatarioNombre;
                d.Calle = destinatarioCalle;
                d.Numero = destinatarioNumero;
                d.Avenida = destinatarioAvenida;
                d.Colonia = destinatarioColonia;
                d.Cp = destinatarioCp;
                d.Ciudad = destinatarioCiudad;
                d.Estado = destinatarioEstado;
                d.Referencia = destinatarioReferencia;
                d.Telefono = destinatarioTelefono;
                d.Correo = destinatarioCorreo;
                d.Persona = destinatarioPersona;
                if (idDestinatario != 0) { d.Update(); } else { d.Save(); }
                

                Orden o = idOrden == 0 ? new Orden() : Orden.ObtenerPorId(idOrden);
                o.Folio = ordenFolio;
                o.Fecha = ordenFecha;
                o.Cliente = c;
                o.Destinatario = d;
                o.Usuario = u;
                o.Paquete = p;
                o.Precio = ordenPrecio;
                o.NumeroRastreo = ordenNumRastreo;
                o.Estado = (Estado) ordenEstado;

                if (idOrden != 0)
                {
                    o.Update();
                }
                else
                {
                    o.Save();
                }
                realizado = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return realizado;
        }

        public static bool Eliminar(int id)
        {
            bool realizado = false;
            try
            {
                Orden o = ObtenerPorId(id);
                Paquete p = o.Paquete;
                o.Delete();
                p.Delete();
                realizado = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return realizado;
        }
    }
}

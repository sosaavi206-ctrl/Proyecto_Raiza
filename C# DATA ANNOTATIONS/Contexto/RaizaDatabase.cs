using Microsoft.EntityFrameworkCore;
using RAIZA.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace RAIZA.Data
{
    public class DatabaseService : DbContext
    {
        public DatabaseService(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Tematica> Tematica { get; set; }
        public DbSet<Tarea> Tarea { get; set; }
        public DbSet<ProgresoLeccion> ProgresoLeccion { get; set; }
        public DbSet<Progreso> Progreso { get; set; }
        public DbSet<PedidoKit> PedidoKit { get; set; }
        public DbSet<Notificacion> Notificacion { get; set; }
        public DbSet<Modulo> Modulo { get; set; }
        public DbSet<Leccion> Leccion { get; set; }
        public DbSet<Instructor> Instructor { get; set; }
        public DbSet<Estudiante> Estudiante { get; set; }
        public DbSet<EntregaTarea> EntregaTarea { get; set; }
        public DbSet<Compra> Compra { get; set; }
        public DbSet<Class_Kit> Class_Kit { get; set; }
        public DbSet<ClasesEnVivo> ClasesEnVivo { get; set; }
        public DbSet<ClaseParticipante> ClaseParticipante { get; set; }
        public DbSet<Certificado> Certificado { get; set; }
        public DbSet<Administrador> Administrador { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            EntityConfiguration(modelBuilder);
        }

        private void EntityConfiguration(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<Usuario>().HasKey(u => u.Id);
            modelBuilder.Entity<Usuario>().Property(u => u.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            modelBuilder.Entity<Usuario>().Property(u => u.Nombre).HasColumnName("Nombre");
            modelBuilder.Entity<Usuario>().Property(u => u.Correo).HasColumnName("Correo");
            modelBuilder.Entity<Usuario>().Property(u => u.ContrasenaHash).HasColumnName("Contrasena_hash");
            modelBuilder.Entity<Usuario>().Property(u => u.Rol).HasColumnName("Rol");
            modelBuilder.Entity<Usuario>().Property(u => u.Estado).HasColumnName("Estado");
            // Datos de perfil (se añadieron a la BD; ver ALTER en SEGURIDAD.md)
            modelBuilder.Entity<Usuario>().Property(u => u.Telefono).HasColumnName("telefono");
            modelBuilder.Entity<Usuario>().Property(u => u.Direccion).HasColumnName("direccion");


            modelBuilder.Entity<Tematica>().ToTable("tematica");
            modelBuilder.Entity<Tematica>().HasKey(t => t.idtematica);
            modelBuilder.Entity<Tematica>().Property(t => t.idtematica).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<Tematica>().Property(t => t.Nombre).HasColumnName("nombre");
            modelBuilder.Entity<Tematica>().Property(t => t.ImagenPortada).HasColumnName("imagen_portada");

            modelBuilder.Entity<Tarea>().ToTable("tarea");
            modelBuilder.Entity<Tarea>().HasKey(t => t.idtarea);
            modelBuilder.Entity<Tarea>().Property(t => t.idtarea).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<Tarea>().Property(t => t.Titulo).HasColumnName("titulo");
            modelBuilder.Entity<Tarea>().Property(t => t.Descripcion).HasColumnName("descripcion");
            modelBuilder.Entity<Tarea>().Property(t => t.FechaEntrega).HasColumnName("fecha_entrega");
            modelBuilder.Entity<Tarea>().Property(t => t.idmodulo).HasColumnName("id_modulo");

            modelBuilder.Entity<ProgresoLeccion>().ToTable("progreso_leccion");
            modelBuilder.Entity<ProgresoLeccion>().HasKey(p => p.Idprogresoleccion);
            modelBuilder.Entity<ProgresoLeccion>().Property(p => p.Idprogresoleccion).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<ProgresoLeccion>().Property(p => p.completado).HasColumnName("completado");
            modelBuilder.Entity<ProgresoLeccion>().Property(p => p.fecha).HasColumnName("fecha");
            modelBuilder.Entity<ProgresoLeccion>().Property(p => p.Idestudiante).HasColumnName("id_estudiante");
            modelBuilder.Entity<ProgresoLeccion>().Property(p => p.Idleccion).HasColumnName("id_leccion");

            modelBuilder.Entity<Progreso>().ToTable("progreso");
            modelBuilder.Entity<Progreso>().HasKey(p => p.Idprogreso);
            modelBuilder.Entity<Progreso>().Property(p => p.Idprogreso).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<Progreso>().Property(p => p.Completado).HasColumnName("completado");
            modelBuilder.Entity<Progreso>().Property(p => p.Porcentaje).HasColumnName("porcentaje");
            modelBuilder.Entity<Progreso>().Property(p => p.FechaCompletado).HasColumnName("fecha_completado");
            modelBuilder.Entity<Progreso>().Property(p => p.idestudiante).HasColumnName("id_estudiante");
            modelBuilder.Entity<Progreso>().Property(p => p.Idmodulo).HasColumnName("id_modulo");

            modelBuilder.Entity<PedidoKit>().ToTable("pedido_kit");
            modelBuilder.Entity<PedidoKit>().HasKey(p => p.idPedidoKit);
            modelBuilder.Entity<PedidoKit>().Property(p => p.idPedidoKit).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<PedidoKit>().Property(p => p.Cantidad).HasColumnName("cantidad");
            modelBuilder.Entity<PedidoKit>().Property(p => p.Estado).HasColumnName("estado");
            modelBuilder.Entity<PedidoKit>().Property(p => p.Direccionenvio).HasColumnName("direccion_envio");
            modelBuilder.Entity<PedidoKit>().Property(p => p.Fechapedido).HasColumnName("fecha_pedido");
            modelBuilder.Entity<PedidoKit>().Property(p => p.Idestudiante).HasColumnName("id_estudiante");
            modelBuilder.Entity<PedidoKit>().Property(p => p.idclasskit).HasColumnName("id_class_kit");
            modelBuilder.Entity<PedidoKit>().Property(p => p.idcompra).HasColumnName("id_compra");

            modelBuilder.Entity<Notificacion>().ToTable("Notificacion");
            modelBuilder.Entity<Notificacion>().HasKey(n => n.Idnotificacion);
            modelBuilder.Entity<Notificacion>().Property(n => n.Idnotificacion).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<Notificacion>().Property(n => n.tiponotificacion).HasColumnName("tipo_notificacion");
            modelBuilder.Entity<Notificacion>().Property(n => n.mensaje).HasColumnName("mensaje");
            modelBuilder.Entity<Notificacion>().Property(n => n.estadoleido).HasColumnName("estado_leido");
            modelBuilder.Entity<Notificacion>().Property(n => n.fechaenvivo).HasColumnName("fecha_envio");
            modelBuilder.Entity<Notificacion>().Property(n => n.idusuario).HasColumnName("id_usuario");

            modelBuilder.Entity<Modulo>().ToTable("modulo");
            modelBuilder.Entity<Modulo>().HasKey(m => m.idmodulo);
            modelBuilder.Entity<Modulo>().Property(m => m.idmodulo).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<Modulo>().Property(m => m.Nivel).HasColumnName("nivel");
            modelBuilder.Entity<Modulo>().Property(m => m.Precio).HasColumnName("precio");
            modelBuilder.Entity<Modulo>().Property(m => m.IncluyeKit).HasColumnName("incluye_kit");
            modelBuilder.Entity<Modulo>().Property(m => m.idtematica).HasColumnName("id_tematica");
            modelBuilder.Entity<Modulo>().Property(m => m.idinstructor).HasColumnName("id_instructor");

            modelBuilder.Entity<Leccion>().ToTable("leccion");
            modelBuilder.Entity<Leccion>().HasKey(l => l.idleccion);
            modelBuilder.Entity<Leccion>().Property(l => l.idleccion).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<Leccion>().Property(l => l.Titulo).HasColumnName("titulo");
            modelBuilder.Entity<Leccion>().Property(l => l.Tipo).HasColumnName("tipo");
            modelBuilder.Entity<Leccion>().Property(l => l.Orden).HasColumnName("orden");
            modelBuilder.Entity<Leccion>().Property(l => l.idmodulo).HasColumnName("id_modulo");

            modelBuilder.Entity<Instructor>().ToTable("instructor");
            modelBuilder.Entity<Instructor>().HasKey(i => i.idinstructor);
            modelBuilder.Entity<Instructor>().Property(i => i.idinstructor).HasColumnName("id").ValueGeneratedNever(); // id NO es identity: convención id = usuario.Id
            modelBuilder.Entity<Instructor>().Property(i => i.Especialidad).HasColumnName("especialidad");
            modelBuilder.Entity<Instructor>().Property(i => i.Biografia).HasColumnName("biografia");

            modelBuilder.Entity<Estudiante>().ToTable("estudiante");
            modelBuilder.Entity<Estudiante>().HasKey(e => e.idestudiante);
            // IMPORTANTE: idestudiante NO es identity en la BD (sys.identity_columns solo incluye
            // usuario.id). El registro crea la fila con el MISMO id del usuario para conservar el
            // invariante estudiante.id == usuario.Id que asumen el frontend y los claims del JWT.
            modelBuilder.Entity<Estudiante>().Property(e => e.idestudiante).HasColumnName("id").ValueGeneratedNever();
            modelBuilder.Entity<Estudiante>().Property(e => e.Espremium).HasColumnName("es_premium");
            modelBuilder.Entity<Estudiante>().Property(e => e.FechaAcceso).HasColumnName("fecha_acceso");

            modelBuilder.Entity<EntregaTarea>().ToTable("entrega_tarea");
            modelBuilder.Entity<EntregaTarea>().HasKey(e => e.Identregatarea);
            modelBuilder.Entity<EntregaTarea>().Property(e => e.Identregatarea).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<EntregaTarea>().Property(e => e.UrlArchivo).HasColumnName("url_archivo");
            modelBuilder.Entity<EntregaTarea>().Property(e => e.FechaEntrega).HasColumnName("fecha_entrega");
            modelBuilder.Entity<EntregaTarea>().Property(e => e.Calificacion).HasColumnName("calificacion");
            modelBuilder.Entity<EntregaTarea>().Property(e => e.Comentario).HasColumnName("comentario");
            modelBuilder.Entity<EntregaTarea>().Property(e => e.idtarea).HasColumnName("id_tarea");
            modelBuilder.Entity<EntregaTarea>().Property(e => e.idestudiante).HasColumnName("id_estudiante");
            modelBuilder.Entity<EntregaTarea>().Property(e => e.idinstructorcalifica).HasColumnName("id_instructor_califica");

            modelBuilder.Entity<Compra>().ToTable("compra");
            modelBuilder.Entity<Compra>().HasKey(c => c.idcompra);
            modelBuilder.Entity<Compra>().Property(c => c.idcompra).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<Compra>().Property(c => c.Monto).HasColumnName("monto");
            modelBuilder.Entity<Compra>().Property(c => c.MetodoPago).HasColumnName("metodo_pago");
            modelBuilder.Entity<Compra>().Property(c => c.Estado).HasColumnName("estado");
            modelBuilder.Entity<Compra>().Property(c => c.ReferenciaWompi).HasColumnName("referencia_wompi");
            modelBuilder.Entity<Compra>().Property(c => c.FechaCompra).HasColumnName("fecha_compra");
            modelBuilder.Entity<Compra>().Property(c => c.idestudiante).HasColumnName("id_estudiante");
            modelBuilder.Entity<Compra>().Property(c => c.idmodulo).HasColumnName("id_modulo");

            modelBuilder.Entity<Class_Kit>().ToTable("class_kit");
            modelBuilder.Entity<Class_Kit>().HasKey(c => c.idclass_kit);
            modelBuilder.Entity<Class_Kit>().Property(c => c.idclass_kit).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<Class_Kit>().Property(c => c.name).HasColumnName("nombre");
            modelBuilder.Entity<Class_Kit>().Property(c => c.description).HasColumnName("descripcion");
            modelBuilder.Entity<Class_Kit>().Property(c => c.precio).HasColumnName("precio");
            modelBuilder.Entity<Class_Kit>().Property(c => c.stockdisponible).HasColumnName("stock_disponible");
            modelBuilder.Entity<Class_Kit>().Property(c => c.tipo).HasColumnName("tipo");
            modelBuilder.Entity<Class_Kit>().Property(c => c.idmodulo).HasColumnName("id_modulo");

            modelBuilder.Entity<ClasesEnVivo>().ToTable("clase_en_vivo");
            modelBuilder.Entity<ClasesEnVivo>().HasKey(c => c.idclaasesenvivo);
            modelBuilder.Entity<ClasesEnVivo>().Property(c => c.idclaasesenvivo).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<ClasesEnVivo>().Property(c => c.FechaHora).HasColumnName("fecha_hora");
            modelBuilder.Entity<ClasesEnVivo>().Property(c => c.UrlSala).HasColumnName("url_sala");
            modelBuilder.Entity<ClasesEnVivo>().Property(c => c.Estado).HasColumnName("estado");
            modelBuilder.Entity<ClasesEnVivo>().Property(c => c.idmodulo).HasColumnName("id_modulo");
            modelBuilder.Entity<ClasesEnVivo>().Property(c => c.idinstructor).HasColumnName("id_instructor");

            modelBuilder.Entity<ClaseParticipante>().ToTable("Clase_Participante");
            modelBuilder.Entity<ClaseParticipante>().HasKey(cp => new { cp.idclase, cp.idestudiante });
            modelBuilder.Entity<ClaseParticipante>().Property(cp => cp.idclase).HasColumnName("id_clase");
            modelBuilder.Entity<ClaseParticipante>().Property(cp => cp.idestudiante).HasColumnName("id_estudiante");
            modelBuilder.Entity<ClaseParticipante>().Property(cp => cp.FechaIngreso).HasColumnName("fecha_ingreso");

            modelBuilder.Entity<Certificado>().ToTable("certificado"); 
            modelBuilder.Entity<Certificado>().HasKey(c => c.idcertificado);
            modelBuilder.Entity<Certificado>().Property(c => c.idcertificado).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<Certificado>().Property(c => c.FechaEmision).HasColumnName("fecha_emision");
            modelBuilder.Entity<Certificado>().Property(c => c.UrlPdf).HasColumnName("url_pdf");
            modelBuilder.Entity<Certificado>().Property(c => c.CodigoVerificacion).HasColumnName("codigo_verificacion");
            modelBuilder.Entity<Certificado>().Property(c => c.idestudiante).HasColumnName("id_estudiante");
            modelBuilder.Entity<Certificado>().Property(c => c.idmodulo).HasColumnName("id_modulo");

            modelBuilder.Entity<Administrador>().ToTable("administrador");
            modelBuilder.Entity<Administrador>().HasKey(a => a.idadministrador);
            modelBuilder.Entity<Administrador>().Property(a => a.idadministrador).HasColumnName("id").ValueGeneratedNever(); // id NO es identity: convención id = usuario.Id
            modelBuilder.Entity<Administrador>().Property(a => a.NivelAcceso).HasColumnName("nivel_acceso");
        }


        public async Task<bool> SaveAsync()
        {
            return await SaveChangesAsync() > 0;
        }
    }
}
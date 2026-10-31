using System;
using System.Collections.Generic;
using LEN_API.Datos.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LEN_API.Datos;

public partial class ContextoLen : DbContext
{
    public ContextoLen(DbContextOptions<ContextoLen> options)
        : base(options)
    {
    }

    public virtual DbSet<TB_BITACORA> TB_BITACORA { get; set; }

    public virtual DbSet<TB_CANTON> TB_CANTON { get; set; }

    public virtual DbSet<TB_CARRITO> TB_CARRITO { get; set; }

    public virtual DbSet<TB_CATEGORIA> TB_CATEGORIA { get; set; }

    public virtual DbSet<TB_DETALLE_CARRITO> TB_DETALLE_CARRITO { get; set; }

    public virtual DbSet<TB_DETALLE_PEDIDO> TB_DETALLE_PEDIDO { get; set; }

    public virtual DbSet<TB_DIRECCION_ENVIO> TB_DIRECCION_ENVIO { get; set; }

    public virtual DbSet<TB_DISTRITO> TB_DISTRITO { get; set; }

    public virtual DbSet<TB_ERROR> TB_ERROR { get; set; }

    public virtual DbSet<TB_ESTADO_FACTURA> TB_ESTADO_FACTURA { get; set; }

    public virtual DbSet<TB_ESTADO_PAGO> TB_ESTADO_PAGO { get; set; }

    public virtual DbSet<TB_ESTADO_PEDIDO> TB_ESTADO_PEDIDO { get; set; }

    public virtual DbSet<TB_FACTURA> TB_FACTURA { get; set; }

    public virtual DbSet<TB_FAVORITO> TB_FAVORITO { get; set; }

    public virtual DbSet<TB_IMAGEN_PRODUCTO> TB_IMAGEN_PRODUCTO { get; set; }

    public virtual DbSet<TB_METODO_PAGO> TB_METODO_PAGO { get; set; }

    public virtual DbSet<TB_PAGO> TB_PAGO { get; set; }

    public virtual DbSet<TB_PEDIDO> TB_PEDIDO { get; set; }

    public virtual DbSet<TB_PRODUCTO> TB_PRODUCTO { get; set; }

    public virtual DbSet<TB_PROVINCIA> TB_PROVINCIA { get; set; }

    public virtual DbSet<TB_ROL> TB_ROL { get; set; }

    public virtual DbSet<TB_USUARIO> TB_USUARIO { get; set; }

    public virtual DbSet<TB_VARIANTE_PRODUCTO> TB_VARIANTE_PRODUCTO { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_100_CI_AI_SC_UTF8");

        modelBuilder.Entity<TB_BITACORA>(entity =>
        {
            entity.HasKey(e => e.IdBitacora).HasName("PK_Bitacora");

            entity.HasIndex(e => new { e.Entidad, e.IdRegistro, e.FechaHora }, "IX_Bitacora_Entidad_Registro").IsDescending(false, false, true);

            entity.HasIndex(e => e.FechaHora, "IX_Bitacora_FechaHora").IsDescending();

            entity.Property(e => e.Accion).HasMaxLength(50);
            entity.Property(e => e.Entidad).HasMaxLength(100);
            entity.Property(e => e.FechaHora)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Bitacora_FechaHora");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TB_BITACORA)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_Bitacora_Usuario");
        });

        modelBuilder.Entity<TB_CANTON>(entity =>
        {
            entity.HasKey(e => e.IdCanton).HasName("PK_Canton");

            entity.HasIndex(e => new { e.IdProvincia, e.Nombre }, "UQ_Canton_Provincia_Nombre").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.IdProvinciaNavigation).WithMany(p => p.TB_CANTON)
                .HasForeignKey(d => d.IdProvincia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Canton_Provincia");
        });

        modelBuilder.Entity<TB_CARRITO>(entity =>
        {
            entity.HasKey(e => e.IdCarrito).HasName("PK_Carrito");

            entity.HasIndex(e => e.IdUsuario, "UX_Carrito_UsuarioActivo")
                .IsUnique()
                .HasFilter("([Activo]=(1))");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Carrito_Activo");
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Carrito_FechaCreacion");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.TB_CARRITO)
                .HasForeignKey<TB_CARRITO>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Carrito_Usuario");
        });

        modelBuilder.Entity<TB_CATEGORIA>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK_Categoria");

            entity.HasIndex(e => e.Nombre, "UQ_Categoria_Nombre").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Categoria_Activo");
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<TB_DETALLE_CARRITO>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCarrito).HasName("PK_DetalleCarrito");

            entity.HasIndex(e => new { e.IdCarrito, e.IdProducto }, "UQ_DetalleCarrito_Carrito_Producto").IsUnique();

            entity.HasOne(d => d.IdCarritoNavigation).WithMany(p => p.TB_DETALLE_CARRITO)
                .HasForeignKey(d => d.IdCarrito)
                .HasConstraintName("FK_DetalleCarrito_Carrito");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.TB_DETALLE_CARRITO)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCarrito_Producto");
        });

        modelBuilder.Entity<TB_DETALLE_PEDIDO>(entity =>
        {
            entity.HasKey(e => e.IdDetallePedido).HasName("PK_DetallePedido");

            entity.HasIndex(e => new { e.IdPedido, e.IdProducto }, "UQ_DetallePedido_Pedido_Producto").IsUnique();

            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SubtotalLinea).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.TB_DETALLE_PEDIDO)
                .HasForeignKey(d => d.IdPedido)
                .HasConstraintName("FK_DetallePedido_Pedido");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.TB_DETALLE_PEDIDO)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePedido_Producto");
        });

        modelBuilder.Entity<TB_DIRECCION_ENVIO>(entity =>
        {
            entity.HasKey(e => e.IdDireccion).HasName("PK_DireccionEnvio");

            entity.HasIndex(e => new { e.IdUsuario, e.Activo }, "IX_DireccionEnvio_Usuario");

            entity.HasIndex(e => e.IdUsuario, "UX_DireccionEnvio_PrincipalUsuario")
                .IsUnique()
                .HasFilter("([EsPrincipal]=(1) AND [Activo]=(1))");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_DireccionEnvio_Activo");
            entity.Property(e => e.DireccionExacta).HasMaxLength(300);
            entity.Property(e => e.NombreDestinatario).HasMaxLength(150);
            entity.Property(e => e.Referencia).HasMaxLength(300);
            entity.Property(e => e.TelefonoContacto).HasMaxLength(20);

            entity.HasOne(d => d.IdDistritoNavigation).WithMany(p => p.TB_DIRECCION_ENVIO)
                .HasForeignKey(d => d.IdDistrito)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DireccionEnvio_Distrito");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.TB_DIRECCION_ENVIO)
                .HasForeignKey<TB_DIRECCION_ENVIO>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DireccionEnvio_Usuario");
        });

        modelBuilder.Entity<TB_DISTRITO>(entity =>
        {
            entity.HasKey(e => e.IdDistrito).HasName("PK_Distrito");

            entity.HasIndex(e => new { e.IdCanton, e.Nombre }, "UQ_Distrito_Canton_Nombre").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.IdCantonNavigation).WithMany(p => p.TB_DISTRITO)
                .HasForeignKey(d => d.IdCanton)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Distrito_Canton");
        });

        modelBuilder.Entity<TB_ERROR>(entity =>
        {
            entity.HasKey(e => e.IdError).HasName("PK_Error");

            entity.HasIndex(e => e.FechaError, "IX_Error_FechaError").IsDescending();

            entity.Property(e => e.FechaError)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Error_FechaError");
            entity.Property(e => e.Metodo).HasMaxLength(150);
            entity.Property(e => e.Origen).HasMaxLength(150);
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.UsuarioSistema).HasMaxLength(150);
        });

        modelBuilder.Entity<TB_ESTADO_FACTURA>(entity =>
        {
            entity.HasKey(e => e.IdEstadoFactura).HasName("PK_EstadoFactura");

            entity.HasIndex(e => e.Nombre, "UQ_EstadoFactura_Nombre").IsUnique();

            entity.Property(e => e.IdEstadoFactura).ValueGeneratedNever();
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TB_ESTADO_PAGO>(entity =>
        {
            entity.HasKey(e => e.IdEstadoPago).HasName("PK_EstadoPago");

            entity.HasIndex(e => e.Nombre, "UQ_EstadoPago_Nombre").IsUnique();

            entity.Property(e => e.IdEstadoPago).ValueGeneratedNever();
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TB_ESTADO_PEDIDO>(entity =>
        {
            entity.HasKey(e => e.IdEstadoPedido).HasName("PK_EstadoPedido");

            entity.HasIndex(e => e.Nombre, "UQ_EstadoPedido_Nombre").IsUnique();

            entity.Property(e => e.IdEstadoPedido).ValueGeneratedNever();
            entity.Property(e => e.ClaseCss).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TB_FACTURA>(entity =>
        {
            entity.HasKey(e => e.IdFactura).HasName("PK_Factura");

            entity.HasIndex(e => e.NumeroFactura, "UQ_Factura_Numero").IsUnique();

            entity.HasIndex(e => e.IdPedido, "UQ_Factura_Pedido").IsUnique();

            entity.Property(e => e.CostoEnvio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FechaFactura)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Factura_FechaFactura");
            entity.Property(e => e.NumeroFactura).HasMaxLength(50);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdEstadoFacturaNavigation).WithMany(p => p.TB_FACTURA)
                .HasForeignKey(d => d.IdEstadoFactura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_EstadoFactura");

            entity.HasOne(d => d.IdPedidoNavigation).WithOne(p => p.TB_FACTURA)
                .HasForeignKey<TB_FACTURA>(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_Pedido");
        });

        modelBuilder.Entity<TB_FAVORITO>(entity =>
        {
            entity.HasKey(e => e.IdFavorito).HasName("PK_Favorito");

            entity.HasIndex(e => new { e.IdUsuario, e.IdProducto }, "UQ_Favorito_Usuario_Producto").IsUnique();

            entity.Property(e => e.FechaAgregado)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Favorito_FechaAgregado");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.TB_FAVORITO)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favorito_Producto");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TB_FAVORITO)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favorito_Usuario");
        });

        modelBuilder.Entity<TB_IMAGEN_PRODUCTO>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PK_ImagenProducto");

            entity.HasIndex(e => new { e.IdProducto, e.Activo }, "IX_ImagenProducto_Producto");

            entity.HasIndex(e => e.IdProducto, "UX_ImagenProducto_PrincipalProducto")
                .IsUnique()
                .HasFilter("([EsPrincipal]=(1) AND [Activo]=(1))");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_ImagenProducto_Activo");
            entity.Property(e => e.RutaImagen).HasMaxLength(255);
            entity.Property(e => e.TextoAlternativo).HasMaxLength(150);

            entity.HasOne(d => d.IdProductoNavigation).WithOne(p => p.TB_IMAGEN_PRODUCTO)
                .HasForeignKey<TB_IMAGEN_PRODUCTO>(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImagenProducto_Producto");
        });

        modelBuilder.Entity<TB_METODO_PAGO>(entity =>
        {
            entity.HasKey(e => e.IdMetodoPago).HasName("PK_MetodoPago");

            entity.HasIndex(e => e.Nombre, "UQ_MetodoPago_Nombre").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_MetodoPago_Activo");
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.EtiquetaDatoPago).HasMaxLength(100);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<TB_PAGO>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PK_Pago");

            entity.HasIndex(e => e.IdPedido, "IX_Pago_Pedido");

            entity.Property(e => e.Comprobante).HasMaxLength(255);
            entity.Property(e => e.FechaPago)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Pago_FechaPago");
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdEstadoPagoNavigation).WithMany(p => p.TB_PAGO)
                .HasForeignKey(d => d.IdEstadoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_EstadoPago");

            entity.HasOne(d => d.IdMetodoPagoNavigation).WithMany(p => p.TB_PAGO)
                .HasForeignKey(d => d.IdMetodoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_MetodoPago");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.TB_PAGO)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_Pedido");
        });

        modelBuilder.Entity<TB_PEDIDO>(entity =>
        {
            entity.HasKey(e => e.IdPedido).HasName("PK_Pedido");

            entity.HasIndex(e => e.FechaPedido, "IX_Pedido_Fecha");

            entity.HasIndex(e => new { e.IdUsuario, e.FechaPedido }, "IX_Pedido_Usuario_Fecha").IsDescending(false, true);

            entity.Property(e => e.CostoEnvio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FechaPedido)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Pedido_FechaPedido");
            entity.Property(e => e.MotivoCancelacion).HasMaxLength(255);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdDireccionNavigation).WithMany(p => p.TB_PEDIDO)
                .HasForeignKey(d => d.IdDireccion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedido_DireccionEnvio");

            entity.HasOne(d => d.IdEstadoPedidoNavigation).WithMany(p => p.TB_PEDIDO)
                .HasForeignKey(d => d.IdEstadoPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedido_EstadoPedido");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TB_PEDIDO)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedido_Usuario");
        });

        modelBuilder.Entity<TB_PRODUCTO>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK_Producto");

            entity.HasIndex(e => new { e.IdCategoria, e.Activo }, "IX_Producto_Categoria");

            entity.HasIndex(e => new { e.Destacado, e.Activo }, "IX_Producto_Destacado").HasFilter("([Destacado]=(1))");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Producto_Activo");
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Producto_FechaCreacion");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.TB_PRODUCTO)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Producto_Categoria");
        });

        modelBuilder.Entity<TB_PROVINCIA>(entity =>
        {
            entity.HasKey(e => e.IdProvincia).HasName("PK_Provincia");

            entity.HasIndex(e => e.Nombre, "UQ_Provincia_Nombre").IsUnique();

            entity.Property(e => e.IdProvincia).ValueGeneratedNever();
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<TB_ROL>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK_Rol");

            entity.HasIndex(e => e.Nombre, "UQ_Rol_Nombre").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Rol_Activo");
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TB_USUARIO>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK_Usuario");

            entity.HasIndex(e => e.Correo, "UQ_Usuario_Correo").IsUnique();

            entity.HasIndex(e => e.Cedula, "UX_Usuario_Cedula")
                .IsUnique()
                .HasFilter("([Cedula] IS NOT NULL)");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Usuario_Activo");
            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Cedula).HasMaxLength(20);
            entity.Property(e => e.ContrasenaHash).HasMaxLength(255);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.FechaRegistro)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Usuario_FechaRegistro");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.VigenciaContrasenaTemporal).HasPrecision(0);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.TB_USUARIO)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");
        });

        modelBuilder.Entity<TB_VARIANTE_PRODUCTO>(entity =>
        {
            entity.HasKey(e => e.IdVariante).HasName("PK_VarianteProducto");

            entity.HasIndex(e => new { e.IdProducto, e.Color, e.Talla, e.Modelo }, "UQ_VarianteProducto_Combinacion").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_VarianteProducto_Activo");
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Modelo).HasMaxLength(50);
            entity.Property(e => e.Talla).HasMaxLength(50);

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.TB_VARIANTE_PRODUCTO)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VarianteProducto_Producto");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

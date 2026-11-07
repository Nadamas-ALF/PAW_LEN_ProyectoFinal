// ====================================================================
// len.js - Funciones de interfaz de LEN Accessories
//
// Solo interfaz. Ninguna regla de negocio vive aca: validar stock,
// calcular totales o decidir si una transicion de estado es valida es
// del API y de los procedimientos.
//
// Lo que todavia no tiene servidor avisa que falta. Nunca finge que
// guardo algo (RG-123).
// ====================================================================

(function () {
    'use strict';

    // Los mensajes llevan los acentos como entidades y el archivo se queda en
    // ASCII, igual que las vistas y los stubs de los controladores. Por eso se
    // pintan con innerHTML y no con textContent, y por eso ninguno puede traer
    // texto que venga de fuera
    var PENDIENTE = 'Esta acci&oacute;n todav&iacute;a no est&aacute; conectada: falta el servidor.';

    // ----------------------------------------------------------------
    // Aviso en pantalla
    // ----------------------------------------------------------------

    // Mete un LenAviso arriba del contenido. Se usa para todo lo que
    // depende de un endpoint que no existe: asi el boton dice la verdad
    // en lugar de no hacer nada
    function avisar(mensaje) {
        var contenido = document.getElementById('contenido');

        if (!contenido) {
            return;
        }

        var anterior = contenido.querySelector('.LenAviso[data-origen="len.js"]');
        if (anterior) {
            anterior.remove();
        }

        var aviso = document.createElement('div');
        aviso.className = 'LenAviso LenAviso--Aviso alert fade show mb-4';
        aviso.setAttribute('role', 'alert');
        aviso.setAttribute('data-origen', 'len.js');
        aviso.innerHTML =
            '<span class="LenAviso-Icono" aria-hidden="true"><i class="bi bi-exclamation-circle"></i></span>' +
            '<div class="LenAviso-Cuerpo"><p class="LenAviso-Titulo mb-0"></p></div>' +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Cerrar"></button>';

        aviso.querySelector('.LenAviso-Titulo').innerHTML = mensaje;

        var primero = contenido.firstElementChild;
        if (primero) {
            contenido.insertBefore(aviso, primero.nextSibling);
        } else {
            contenido.appendChild(aviso);
        }

        aviso.scrollIntoView({ block: 'nearest' });
    }

    // ----------------------------------------------------------------
    // 1. Mostrar u ocultar la contrasena
    // ----------------------------------------------------------------

    // Sale en IniciarSesion, Registro y Admin/UsuarioDetalle. El boton no
    // lleva data- de enganche: se reconoce por estar dentro del mismo
    // LenCampoGrupo que un campo de contrasena
    function contrasenasVisibles() {
        var grupos = document.querySelectorAll('.LenCampoGrupo');

        Array.prototype.forEach.call(grupos, function (grupo) {
            var campo = grupo.querySelector('input[type="password"]');
            var boton = grupo.querySelector('button[type="button"]');

            if (!campo || !boton) {
                return;
            }

            boton.addEventListener('click', function () {
                var oculta = campo.type === 'password';
                campo.type = oculta ? 'text' : 'password';

                var icono = boton.querySelector('i');
                if (icono) {
                    icono.className = oculta ? 'bi bi-eye-slash' : 'bi bi-eye';
                }

                boton.setAttribute('aria-pressed', oculta ? 'true' : 'false');
            });
        });
    }

    // ----------------------------------------------------------------
    // 2. Modales compartidas que se llenan con los data- de la fila
    // ----------------------------------------------------------------

    // Cada pantalla tiene una sola modal de editar y muchas filas. El
    // boton de la fila trae los datos en data- y aca se copian a los
    // campos. Sin esto la modal editaria siempre el mismo registro
    var MODALES = [
        {
            id: 'EditarCategoria',
            campos: {
                id: 'input[name="IdCategoria"]',
                nombre: '#EditarNombre',
                descripcion: '#EditarDescripcion'
            }
        },
        {
            id: 'EditarUsuario',
            campos: {
                id: 'input[name="IdUsuario"]',
                nombre: '#EditarNombre',
                apellido: '#EditarApellido',
                cedula: '#EditarCedula',
                telefono: '#EditarTelefono',
                correo: '#EditarCorreo',
                rol: '#EditarIdRol',
                activo: '#EditarActivo'
            }
        },
        {
            id: 'EditarDireccion',
            campos: {
                id: 'input[name="IdDireccion"]',
                destinatario: '#EditarDestinatario',
                telefono: '#EditarTelefono',
                exacta: '#EditarDireccionExacta',
                referencia: '#EditarReferencia',
                provincia: '#EditarProvincia',
                canton: '#EditarCanton',
                distrito: '#EditarDistrito',
                principal: '#EditarEsPrincipal'
            }
        },
        {
            // La de quitar del carrito no tiene campos: lo unico que cambia
            // es el nombre del producto en el texto
            id: 'ConfirmarEliminar',
            campos: {},
            resaltado: 'nombre'
        }
    ];

    function ponerValor(campo, valor) {
        if (!campo) {
            return;
        }

        if (campo.type === 'checkbox') {
            campo.checked = valor === 'true';
            return;
        }

        campo.value = valor === null ? '' : valor;
    }

    function modalesConDatos() {
        MODALES.forEach(function (plantilla) {
            var modal = document.getElementById(plantilla.id);

            if (!modal) {
                return;
            }

            modal.addEventListener('show.bs.modal', function (evento) {
                var disparador = evento.relatedTarget;

                if (!disparador) {
                    return;
                }

                Object.keys(plantilla.campos).forEach(function (clave) {
                    ponerValor(
                        modal.querySelector(plantilla.campos[clave]),
                        disparador.getAttribute('data-' + clave));
                });

                if (plantilla.resaltado) {
                    var resaltado = modal.querySelector('.modal-body strong');
                    if (resaltado) {
                        resaltado.textContent = disparador.getAttribute('data-' + plantilla.resaltado) || '';
                    }
                }

                // El id del registro queda en la modal para el boton que no
                // es de formulario, como el de quitar del carrito
                modal.setAttribute('data-idproducto', disparador.getAttribute('data-idproducto') || '');
            });
        });
    }

    // ----------------------------------------------------------------
    // 3. Metodo de pago del checkout
    // ----------------------------------------------------------------

    // Los cuatro bloques nacen ocultos menos el del metodo elegido. El
    // nombre sale del data-nombre de la opcion, no del texto visible
    function metodoDePago() {
        var selector = document.getElementById('MetodoPago');

        if (!selector) {
            return;
        }

        var bloques = document.querySelectorAll('[data-metodo]');
        var oculto = document.querySelector('input[name="DatoPagoSimulado"]');

        function mostrar() {
            var elegida = selector.options[selector.selectedIndex];
            var nombre = elegida ? elegida.getAttribute('data-nombre') : null;

            Array.prototype.forEach.call(bloques, function (bloque) {
                bloque.hidden = bloque.getAttribute('data-metodo') !== nombre;
            });

            componerDato();
        }

        // Lo unico que se envia de todo el bloque de pago. Se arma del
        // campo visible porque los de cada bloque no llevan name
        function componerDato() {
            if (!oculto) {
                return;
            }

            var visible = null;

            Array.prototype.forEach.call(bloques, function (bloque) {
                if (bloque.hidden) {
                    return;
                }
                var campos = bloque.querySelectorAll('input');
                if (campos.length > 0) {
                    visible = campos[0];
                }
            });

            oculto.value = visible ? visible.value : '';
        }

        selector.addEventListener('change', mostrar);

        Array.prototype.forEach.call(bloques, function (bloque) {
            Array.prototype.forEach.call(bloque.querySelectorAll('input'), function (campo) {
                campo.addEventListener('input', componerDato);
            });
        });

        mostrar();
    }

    // ----------------------------------------------------------------
    // 4. Motivo de cancelacion del pedido
    // ----------------------------------------------------------------

    // Se muestra solo al elegir Cancelado, que es el estado 4. El
    // procedimiento lo exige obligatorio en ese caso
    function motivoDeCancelacion() {
        var selector = document.getElementById('IdEstadoNuevo');
        var motivo = document.getElementById('MotivoCancelacion');

        if (!selector || !motivo) {
            return;
        }

        var bloque = motivo.closest('div');

        selector.addEventListener('change', function () {
            var cancela = selector.value === '4';

            bloque.hidden = !cancela;
            motivo.required = cancela;

            if (!cancela) {
                motivo.value = '';
            }
        });
    }

    // ----------------------------------------------------------------
    // 5. Visor de la imagen completa
    // ----------------------------------------------------------------

    // Sale en Admin/ProductoImagenes. El marcador sin imagen no abre nada
    function visorDeImagenes() {
        var visor = document.getElementById('VisorImagen');

        if (!visor) {
            return;
        }

        var destino = visor.querySelector('img');
        var marcadores = document.querySelectorAll('.LenMarcador--Apaisado');

        Array.prototype.forEach.call(marcadores, function (marcador) {
            var imagen = marcador.querySelector('img');

            if (!imagen) {
                return;
            }

            marcador.style.cursor = 'zoom-in';
            marcador.setAttribute('role', 'button');
            marcador.setAttribute('tabindex', '0');

            function abrir() {
                destino.src = imagen.src;
                destino.alt = imagen.alt;
                bootstrap.Modal.getOrCreateInstance(visor).show();
            }

            marcador.addEventListener('click', abrir);
            marcador.addEventListener('keydown', function (evento) {
                if (evento.key === 'Enter' || evento.key === ' ') {
                    evento.preventDefault();
                    abrir();
                }
            });
        });
    }


    // ----------------------------------------------------------------
    // 6. Cascada de provincia, canton y distrito
    // ----------------------------------------------------------------

    async function consultarUbicaciones(url) {
        var respuesta = await fetch(url, {
            method: 'GET',
            headers: {
                'Accept': 'application/json'
            },
            credentials: 'same-origin'
        });

        if (!respuesta.ok) {
            throw new Error('No fue posible consultar las ubicaciones.');
        }

        return await respuesta.json();
    }

    function opcionVacia(texto) {
        var opcion = document.createElement('option');
        opcion.value = '';
        opcion.textContent = texto;
        return opcion;
    }

    // BT-02. Vacio y deshabilitado, con un texto que dice que nivel falta
    // elegir (CA-3). Va con textContent, por eso el acento como \u00f3
    function reiniciar(selector, texto) {
        selector.innerHTML = '';
        selector.appendChild(opcionVacia(texto));
        selector.disabled = true;
    }

    function llenar(selector, opciones, valorSeleccionado) {
        selector.innerHTML = '';

        selector.appendChild(opcionVacia('Seleccione'));

        opciones.forEach(function (opcion) {
            var elemento = new Option(opcion.nombre, opcion.id);

            if (String(opcion.id) === String(valorSeleccionado)) {
                elemento.selected = true;
            }

            selector.appendChild(elemento);
        });

        selector.disabled = opciones.length === 0;

        if (opciones.length === 0) {
            selector.innerHTML = '';
            selector.appendChild(opcionVacia('No hay opciones disponibles'));
        }
    }

    async function cargarCantones(provincia, canton, distrito, idCanton, idDistrito) {
        reiniciar(canton, 'Seleccione una provincia primero');
        reiniciar(distrito, 'Seleccione un cant\u00f3n primero');

        if (!provincia.value) {
            return;
        }

        try {
            var cantones = await consultarUbicaciones(
                '/Usuario/Cantones?idProvincia=' +
                encodeURIComponent(provincia.value)
            );

            llenar(canton, cantones, idCanton);

            if (canton.value) {
                await cargarDistritos(canton, distrito, idDistrito);
            }
        } catch (error) {
            avisar('No fue posible cargar los cantones. Intente nuevamente.');
        }
    }

    async function cargarDistritos(canton, distrito, idDistrito) {
        reiniciar(distrito, 'Seleccione un cant\u00f3n primero');

        if (!canton.value) {
            return;
        }

        try {
            var distritos = await consultarUbicaciones(
                '/Usuario/Distritos?idCanton=' +
                encodeURIComponent(canton.value)
            );

            llenar(distrito, distritos, idDistrito);
        } catch (error) {
            avisar('No fue posible cargar los distritos. Intente nuevamente.');
        }
    }

    function cascadaDeUbicacion() {
        var provincias = document.querySelectorAll('select[id$="Provincia"]');

        Array.prototype.forEach.call(provincias, function (provincia) {
            var prefijo = provincia.id.replace(/Provincia$/, '');
            var canton = document.getElementById(prefijo + 'Canton');
            var distrito = document.getElementById(prefijo + 'Distrito');

            if (!canton || !distrito) {
                return;
            }

            provincia.addEventListener('change', function () {
                cargarCantones(provincia, canton, distrito, null, null);
            });

            canton.addEventListener('change', function () {
                cargarDistritos(canton, distrito, null);
            });
        });

        // Al abrir la modal de edicion se recuperan las ubicaciones guardadas
        var modalEditar = document.getElementById('EditarDireccion');

        if (modalEditar) {
            modalEditar.addEventListener('show.bs.modal', function (evento) {
                var disparador = evento.relatedTarget;

                if (!disparador) {
                    return;
                }

                var provincia = modalEditar.querySelector('#EditarProvincia');
                var canton = modalEditar.querySelector('#EditarCanton');
                var distrito = modalEditar.querySelector('#EditarDistrito');

                if (!provincia || !canton || !distrito) {
                    return;
                }

                var idProvincia = disparador.getAttribute('data-provincia');
                var idCanton = disparador.getAttribute('data-canton');
                var idDistrito = disparador.getAttribute('data-distrito');

                provincia.value = idProvincia || '';

                cargarCantones(
                    provincia,
                    canton,
                    distrito,
                    idCanton,
                    idDistrito
                );
            });
        }
    }


    // ----------------------------------------------------------------
    // 7. Lo que espera endpoint
    // ----------------------------------------------------------------

    // El carrito y el boton de agregar de la portada necesitan un POST que
    // todavia no existe. En lugar de no hacer nada, avisan
    function accionesSinServidor() {
        var cantidades = document.querySelectorAll('input[data-cantidadoriginal]');

        Array.prototype.forEach.call(cantidades, function (campo) {
            campo.addEventListener('change', function () {
                if (campo.value === campo.getAttribute('data-cantidadoriginal')) {
                    return;
                }

                avisar('La cantidad todav&iacute;a no se puede cambiar: falta el servidor.');
                campo.value = campo.getAttribute('data-cantidadoriginal');
            });
        });

        var quitar = document.getElementById('BotonQuitarDelCarrito');

        if (quitar) {
            quitar.addEventListener('click', function () {
                var modal = document.getElementById('ConfirmarEliminar');
                bootstrap.Modal.getOrCreateInstance(modal).hide();
                avisar('Quitar del carrito todav&iacute;a no est&aacute; conectado: falta el servidor.');
            });
        }

        // El de la portada: la tarjeta del catalogo no lo lleva
        var agregar = document.querySelectorAll('.LenProducto-Acciones button[data-idproducto]');

        Array.prototype.forEach.call(agregar, function (boton) {
            boton.addEventListener('click', function () {
                avisar(PENDIENTE);
            });
        });
    }

    // ----------------------------------------------------------------
    // 7. Confirmacion de acciones sin vuelta atras
    // ----------------------------------------------------------------

    // Condiciones para los formularios que solo confirman en ciertos casos.
    // El nombre sale de data-confirmar-si
    var CONDICIONES = {
        // para el boton que activa en vez de desactivar: comparte formulario con el que
        // si confirma y la vista lo marca asi segun el estado del registro
        nunca: function () {
            return false;
        },
        // el estado 4 es Cancelado, igual que en motivoDeCancelacion
        pedidoCancelado: function (formulario) {
            var estado = formulario.querySelector('#IdEstadoNuevo');
            return !!estado && estado.value === '4';
        },
        // defaultChecked es como llego la casilla: confirma al apagar una cuenta activa
        cuentaDesactivada: function (formulario) {
            var casilla = formulario.querySelector('#Activo');
            return !!casilla && casilla.defaultChecked && !casilla.checked;
        }
    };

    // Los textos salen de atributos y se pintan con textContent, asi un nombre de
    // producto con signos raros no se interpreta como HTML
    function confirmarAcciones() {
        var modal = document.getElementById('ConfirmarAccion');
        var boton = document.getElementById('BotonConfirmarAccion');

        if (!modal || !boton) {
            return;
        }

        var pendiente = null;

        function llenarCuerpo(texto, nombre) {
            var cuerpo = modal.querySelector('.modal-body p');
            var partes = texto.split('{nombre}');

            cuerpo.textContent = '';
            cuerpo.appendChild(document.createTextNode(partes[0]));

            for (var i = 1; i < partes.length; i++) {
                var negrita = document.createElement('strong');
                negrita.textContent = nombre;
                cuerpo.appendChild(negrita);
                cuerpo.appendChild(document.createTextNode(partes[i]));
            }
        }

        document.addEventListener('submit', function (evento) {
            var formulario = evento.target;

            if (!formulario.hasAttribute || !formulario.hasAttribute('data-confirmar-titulo')) {
                return;
            }

            var condicion = formulario.getAttribute('data-confirmar-si');

            if (condicion && !(CONDICIONES[condicion] && CONDICIONES[condicion](formulario))) {
                return;
            }

            evento.preventDefault();
            pendiente = formulario;

            modal.querySelector('.modal-title').textContent = formulario.getAttribute('data-confirmar-titulo');
            llenarCuerpo(formulario.getAttribute('data-confirmar-cuerpo') || '',
                formulario.getAttribute('data-confirmar-nombre') || '');
            boton.textContent = formulario.getAttribute('data-confirmar-boton') || 'Confirmar';

            bootstrap.Modal.getOrCreateInstance(modal).show();
        });

        boton.addEventListener('click', function () {
            var formulario = pendiente;
            pendiente = null;
            bootstrap.Modal.getOrCreateInstance(modal).hide();

            // submit() no dispara el evento de nuevo, asi que no vuelve a preguntar
            if (formulario) {
                formulario.submit();
            }
        });

        modal.addEventListener('hidden.bs.modal', function () {
            pendiente = null;
        });
    }

    // ----------------------------------------------------------------

    document.addEventListener('DOMContentLoaded', function () {
        contrasenasVisibles();
        modalesConDatos();
        metodoDePago();
        motivoDeCancelacion();
        visorDeImagenes();
        cascadaDeUbicacion();
        accionesSinServidor();
        confirmarAcciones();
    });
})();

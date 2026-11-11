// ---------- Favoritos (AJAX) ----------
document.addEventListener('click', async (e) => {
    const btn = e.target.closest('button.btn-favorito');
    if (!btn) return;

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    btn.disabled = true;
    try {
        const resp = await fetch(`/Favoritos/Toggle/${btn.dataset.destino}`, {
            method: 'POST',
            headers: { 'RequestVerificationToken': token }
        });
        if (!resp.ok) throw new Error('Respuesta inválida');
        const data = await resp.json();

        btn.classList.toggle('activo', data.favorito);
        btn.setAttribute('aria-pressed', data.favorito);
        btn.setAttribute('aria-label', data.favorito ? 'Quitar de favoritos' : 'Agregar a favoritos');
        const icono = btn.querySelector('i');
        icono.classList.toggle('fa-solid', data.favorito);
        icono.classList.toggle('fa-regular', !data.favorito);

        Swal.fire({
            toast: true, position: 'top-end', icon: 'success', title: data.mensaje,
            showConfirmButton: false, timer: 2000
        });

        // En "Mis favoritos", al quitarlo desaparece la tarjeta
        if (!data.favorito) btn.closest('.col-fav')?.remove();
    } catch {
        Swal.fire({ icon: 'error', title: 'No se pudo actualizar', text: 'Intentá de nuevo en unos segundos.' });
    } finally {
        btn.disabled = false;
    }
});

// ---------- Detalle del destino y reseñas (modal + AJAX) ----------
const tokenAntiforgery = () => document.querySelector('input[name="__RequestVerificationToken"]')?.value;

function mostrarToast(titulo) {
    Swal.fire({
        toast: true, position: 'top-end', icon: 'success', title: titulo,
        showConfirmButton: false, timer: 2500, timerProgressBar: true
    });
}

async function cargarDetalle(id) {
    const contenedor = document.getElementById('modalDestinoContenido');
    contenedor.innerHTML = '<div class="modal-body text-center py-5"><div class="spinner-border" role="status"><span class="visually-hidden">Cargando…</span></div></div>';
    const resp = await fetch(`/Destinos/Detalle/${id}`);
    contenedor.innerHTML = resp.ok
        ? await resp.text()
        : '<div class="modal-body">No se pudo cargar el destino. Cerrá e intentá de nuevo.</div>';
}

document.addEventListener('click', async (e) => {
    // Abrir el modal
    const abrir = e.target.closest('.btn-detalle');
    if (abrir) {
        bootstrap.Modal.getOrCreateInstance(document.getElementById('modalDestino')).show();
        await cargarDetalle(abrir.dataset.destino);
        return;
    }

    // Eliminar una reseña
    const borrar = e.target.closest('.btn-eliminar-resena');
    if (borrar) {
        const r = await Swal.fire({
            title: '¿Eliminar esta reseña?', text: 'Esta acción no se puede deshacer.', icon: 'warning',
            showCancelButton: true, confirmButtonText: 'Sí, eliminar', cancelButtonText: 'Cancelar'
        });
        if (!r.isConfirmed) return;

        const resp = await fetch(`/Resenas/Eliminar/${borrar.dataset.resena}`, {
            method: 'POST', headers: { 'RequestVerificationToken': tokenAntiforgery() }
        });
        if (resp.ok) {
            mostrarToast((await resp.json()).mensaje);
            await cargarDetalle(borrar.dataset.destino);
        } else {
            Swal.fire({ icon: 'error', title: 'No se pudo eliminar', text: 'Intentá de nuevo.' });
        }
    }
});

// Guardar (crear o editar) la reseña
document.addEventListener('submit', async (e) => {
    const form = e.target.closest('.form-resena');
    if (!form) return;
    e.preventDefault();

    const errores = form.querySelector('.errores-resena');
    errores.textContent = '';

    const resp = await fetch('/Resenas/Guardar', {
        method: 'POST',
        headers: { 'RequestVerificationToken': tokenAntiforgery() },
        body: new FormData(form)
    });

    if (resp.ok) {
        mostrarToast((await resp.json()).mensaje);
        await cargarDetalle(form.dataset.destino);
    } else if (resp.status === 400) {
        errores.textContent = (await resp.json()).errores.join(' ');
    } else {
        errores.textContent = 'No se pudo guardar. Intentá de nuevo.';
    }
});

// ---------- Chatbot flotante (Gemini) ----------
(() => {
    const boton = document.getElementById('chat-boton');
    const panel = document.getElementById('chat-panel');
    if (!boton || !panel) return;

    const cerrar = document.getElementById('chat-cerrar');
    const mensajes = document.getElementById('chat-mensajes');
    const form = document.getElementById('chat-form');
    const entrada = document.getElementById('chat-entrada');
    const historial = [];   // { rol: 'user' | 'model', texto }

    function alternar(abrir) {
        panel.hidden = !abrir;
        boton.setAttribute('aria-expanded', abrir);
        if (abrir) entrada.focus(); else boton.focus();
    }
    boton.addEventListener('click', () => alternar(panel.hidden));
    cerrar.addEventListener('click', () => alternar(false));
    panel.addEventListener('keydown', (e) => { if (e.key === 'Escape') alternar(false); });

    function agregar(texto, clase) {
        const div = document.createElement('div');
        div.className = `chat-msg ${clase}`;
        div.textContent = texto;            // textContent: nunca interpreta HTML
        mensajes.appendChild(div);
        mensajes.scrollTop = mensajes.scrollHeight;
        return div;
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const texto = entrada.value.trim();
        if (!texto) return;

        agregar(texto, 'chat-usuario');
        historial.push({ rol: 'user', texto });
        entrada.value = '';
        entrada.disabled = true;
        const espera = agregar('Escribiendo…', 'chat-bot chat-espera');

        try {
            const resp = await fetch('/Chat/Preguntar', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': tokenAntiforgery() },
                body: JSON.stringify({ mensajes: historial.slice(-10) })
            });
            const data = await resp.json();
            espera.remove();
            agregar(data.respuesta, 'chat-bot');

            if (resp.ok) historial.push({ rol: 'model', texto: data.respuesta });
            else historial.pop();           // la pregunta no se respondió: se descarta del historial
        } catch {
            espera.remove();
            historial.pop();
            agregar('No pude conectarme. Revisá tu conexión e intentá de nuevo.', 'chat-bot');
        } finally {
            entrada.disabled = false;
            entrada.focus();
        }
    });
})();

// Confirmación antes de eliminar (formularios de los ABM)
document.addEventListener('submit', (e) => {
    const form = e.target.closest('.form-eliminar');
    if (!form) return;
    e.preventDefault();
    Swal.fire({
        title: '¿Eliminar este registro?',
        text: 'Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((r) => {
        if (r.isConfirmed) form.submit();
    });
});
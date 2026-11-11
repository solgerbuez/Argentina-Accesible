 // Mensajes de jQuery Validate en español
$.extend($.validator.messages, {
    required: "Este campo es obligatorio.",
    remote: "Por favor, corregí este campo.",
    email: "Ingresá un correo electrónico válido.",
    url: "Ingresá una URL válida.",
    date: "Ingresá una fecha válida.",
    dateISO: "Ingresá una fecha válida (ISO).",
    number: "Ingresá un número válido.",
    digits: "Ingresá solo dígitos.",
    equalTo: "Ingresá el mismo valor otra vez.",
    maxlength: $.validator.format("No ingreses más de {0} caracteres."),
    minlength: $.validator.format("Ingresá al menos {0} caracteres."),
    rangelength: $.validator.format("Ingresá un valor de entre {0} y {1} caracteres."),
    range: $.validator.format("Ingresá un valor entre {0} y {1}."),
    max: $.validator.format("Ingresá un valor menor o igual a {0}."),
    min: $.validator.format("Ingresá un valor mayor o igual a {0}."),
    step: $.validator.format("Ingresá un múltiplo de {0}.")
});
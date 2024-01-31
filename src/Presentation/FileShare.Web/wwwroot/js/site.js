// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

Dropzone.options.FileUpload = {
    paramName: "File",
    maxFilesize: 25, // MB

    success: function (file) {
        window.location.href = file.xhr.responseURL;
    }
};
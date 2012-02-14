// simply force them to call a function that indexes the page. Problem solved

function UpdateIndex() {

    // post to a web service somewhere?
    $.ajax({
        url: document.location.href,
        data: $('body:not(script)').text(),
        success: function (e) {
            // hooray! do we need to even do anything here?
            // The web service needs to use the HtmlReader.StripHtml(source) method
        },
        type: 'POST',
        async: true
    });

}
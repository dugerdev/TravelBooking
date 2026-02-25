$(function() {

    // Check if pickadate is available
    if (typeof $.fn.pickadate !== 'function') {
        return;
    }

    var from_$input = $('.date_from');
    var to_$input = $('.date_to');

    // Check if elements exist
    if (!from_$input.length || !to_$input.length) {
        return;
    }

    try {
        from_$input = from_$input.pickadate();
        var from_picker = from_$input.pickadate('picker');

        to_$input = to_$input.pickadate();
        var to_picker = to_$input.pickadate('picker');

        // Check if pickers are available
        if (!from_picker || !to_picker) {
            return;
        }

        // Check if there's a "from" or "to" date to start with.
        if ( from_picker && from_picker.get('value') ) {
            to_picker.set('min', from_picker.get('select'))
        }
        if ( to_picker && to_picker.get('value') ) {
            from_picker.set('max', to_picker.get('select'))
        }

        // When something is selected, update the "from" and "to" limits.
        from_picker.on('set', function(event) {
            if ( event.select ) {
            to_picker.set('min', from_picker.get('select'))    
            }
            else if ( 'clear' in event ) {
            to_picker.set('min', false)
            }
        })
        to_picker.on('set', function(event) {
            if ( event.select ) {
            from_picker.set('max', to_picker.get('select'))
            }
            else if ( 'clear' in event ) {
            from_picker.set('max', false)
            }
        })
    } catch (e) {
        // Silently fail if pickadate is not properly initialized
        console.warn('Date picker initialization failed:', e);
    }
  
});
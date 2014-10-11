(function ($) {
    $.obear = $.obear || { version: 1.0 };
    $.obear.easyui = $.obear.easyui || {};
})(jQuery);

(function ($) {
    $.obear.tools = {
        formToJson: function (formObj) {
            var o = {};
            var a = formObj.serializeArray();
            $.each(a, function () {

                if (this.value) {
                    if (o[this.name]) {
                        //o[this.name].push(this.value || null);
                        o[this.name] = o[this.name] + '|' + this.value;
                    } else {
                        if ($("[name='" + this.name + "']:checkbox", formObj).length) {
                            //o[this.name] = [this.value];
                            o[this.name] = this.value;
                        } else {
                            o[this.name] = this.value || null;
                        }
                    }
                }
            });
            return o;
        }
    };
})(jQuery);
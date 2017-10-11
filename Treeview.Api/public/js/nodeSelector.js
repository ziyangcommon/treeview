'use strict';
(function ($, window, document, undefined) {
    class NodeSelector{
        constructor(){}
        static init(){
            try{
                let data=TreeviewService.getTree();//
                let selector=$('#nodeSelector');
                let bindNode=function (children,element) {
                    if(children===null ||children===undefined ||element===undefined||element===null){
                        return;
                    }
                    $.each();
                }
            }
            catch (exp){
                console.log(exp);
            }
        }
    }
    NodeSelector.init();
})(jQuery, window, document, undefined);
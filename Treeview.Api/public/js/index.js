'use strict';

/***
 * 树节点管理的服务类
 */
class TreeviewService {
    constructor() {
    }

    /**
     * 获取整个树节点的数据
     * @returns {Array}
     */
    static getTree() {
        // $.ajax({
        //     url:"http://localhost:53986/api/Treeview/GetTree",
        //     success:function (data) {
        //         alert(data);
        //     }
        // });
        let data = $.ajax({
            type: "GET",
            url: "http://localhost:53986/api/Treeview/GetTree",
            async: false
        }).responseText;
        try {
            return JSON.parse(data);
        }
        catch (exp) {
            return [];
        }
    }

    static test() {
    }
}

/***
 * 页面控制器
 */
class TreeviewController {
    constructor() {
    }
}
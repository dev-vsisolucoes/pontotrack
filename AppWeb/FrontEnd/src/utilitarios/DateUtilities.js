import Moment from "moment";

export default class DateUtilities {
    static formatDate(value, format) {
        if(format == undefined) {
            format = "DD/MM/YYYY";
        }
        
        value = Moment(value).format(format);

        return value;
    }

    // static formatDate1(value, format) {
    //     if(format == undefined) {
    //         format = "DD/MM/YYYY HH:mm:ss";
    //     }
        
    //     value = Moment(value).format(format);

    //     return value;
    // }
}
import Moment from "moment";

export default class DateUtilities {
    static formatDate(value, format) {
        if(format == undefined) {
            format = "DD/MM/YYYY";
        }
        
        value = Moment(value).format(format);

        return value;
    }
}
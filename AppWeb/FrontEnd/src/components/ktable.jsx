import React, { Component } from "react";

import Table from "@material-ui/core/Table";
import TableBody from "@material-ui/core/TableBody";
import TableCell from "@material-ui/core/TableCell";
import TableHead from "@material-ui/core/TableHead";
import TableRow from "@material-ui/core/TableRow";

class KTable extends Component {
    render() {
        return(
            // <BootstrapTable data={this.props.data} striped hover>
            //     {props.columns.map(function(value, key) {
            //         return(
            //             <TableHeaderColumn isKey={value.isKey} dataField={value.dataField} dataFormat={value.formatter} columnClassName={value.className} hidden={value.hidden}>
            //                 {value.label}
            //             </TableHeaderColumn>                            
            //         );
            //     })} 
            // </BootstrapTable> 
            <Table size="small">
                <TableHead>
                    <TableRow>
                        {/* <TableCell>Código</TableCell>
                        <TableCell>Descrição</TableCell>
                        <TableCell align="right">Ações</TableCell> */}
                        {this.props.columns.map((column) => (
                            <TableCell
                                key={column.Field}
                                align={column.Align}
                                style={{ width: column.Width }}
                                >
                                {column.Label}
                            </TableCell>
                        ))}                        
                    </TableRow>
                </TableHead>
                <TableBody>
                    {this.props.data.map(row => (
                        // <TableRow key={row.FIAI_CODIGO}>
                        <TableRow key={row[this.props.columns[0].Field]}>
                            {/* <TableCell>{row.FIAI_CODIGO}</TableCell>
                            <TableCell>{row.FIAI_BANCO}</TableCell>
                            <TableCell 
                                >
                                <ButtonGroup 
                                variant="contained" 
                                color="primary" 
                                //ref={anchorRef} 
                                //aria-label="split button"
                                >
                                <Button 
                                    size="small" 
                                    onClick={editButton_HandleClick}
                                    value={row.FIAI_CODIGO}
                                    >
                                    Editar
                                </Button>
                                <Button
                                    color="primary"
                                    size="small"
                                    //aria-controls={open ? 'split-button-menu' : undefined}
                                    //aria-expanded={open ? 'true' : undefined}
                                    //aria-label="select merge strategy"
                                    //aria-haspopup="menu"
                                    value={row.FIAI_CODIGO}
                                    onClick={handleClick}
                                >
                                    <ArrowDropDownIcon />
                                </Button>
                                </ButtonGroup>
                            </TableCell> */}
                            {this.props.columns.map((column) => (
                                <TableCell
                                    //key={column.Field}
                                    align={column.Align}
                                    //style={{ width: column.Width }}
                                    >
                                    {row[column.Field]}
                                    {
                                        //column.Formatter.replace("{" + column.Field + "}", "valter");
                                        //column.Formatter
                                        //React.createClas (column.Formatter)
                                    }
                                </TableCell>
                            ))}                             
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        );
    }
}

export default KTable;

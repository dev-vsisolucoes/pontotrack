import React, { useEffect, useRef, useState } from "react";

//
// Imports MATERIAL-UI
//
import Table from "@material-ui/core/Table";
import TableBody from "@material-ui/core/TableBody";
import TableCell from "@material-ui/core/TableCell";
import TableHead from "@material-ui/core/TableHead";
import TableRow from "@material-ui/core/TableRow";

import {
    IconButton,
    Tooltip,
    TextField,
    MenuItem,
    Menu,
    Chip,
    TableContainer,
    TablePagination, Grid, FormControl, InputLabel, Select
} from "@material-ui/core";

//
// Icones
//
import MoreVertIcon from '@material-ui/icons/MoreVert';
import FilterListIcon from '@material-ui/icons/FilterList';

// 
// Nossos importes
//
import useStyles from "../../theme";
import { Actions } from "../../utilitarios/GeneralUtilities";

export default function EventosLista(props) {

    //
    // Crio o tema da aplicacao, cores, fontes etc
    //    
    const classes = useStyles();

    const [speedDialOpen, setSpeedDialOpen] = React.useState(false);

    return (
        <div style={{ display: (props.action == Actions.LIST ? "block" : "none") }}>
            <Menu
                anchorEl={props.menuActionAnchorRef}
                open={Boolean(props.menuActionAnchorRef)}
                onClose={props.menuActionClose}
                value={props.menuActionAnchorRef}
                keepMounted
            >
                <MenuItem onClick={props.viewClick}>Visualizar</MenuItem>
                <MenuItem onClick={props.editClick}>Reprocessar</MenuItem>
                {/* <MenuItem onClick={props.deleteClick}>Excluir</MenuItem> */}
                {/* <MenuItem onClick={props.printClick}>Imprimir</MenuItem> */}
            </Menu>
            <Grid container >
                <Grid item xs={2}>
                    <TextField
                        variant="standard"
                        label="Buscar por"
                        onChange={props.filterChange}
                    />
                    {/* <Tooltip title="Filtrar">
                        <IconButton
                            aria-label="Filtrar"
                            onClick={props.filterClick}
                        >
                            <FilterListIcon />
                        </IconButton>
                    </Tooltip> */}
                </Grid>
                <Grid item xs={1}>
                    <FormControl variant="standard">
                        <InputLabel htmlFor="standard-age-native-simple">Status</InputLabel>
                        <Select
                            variant="standard"
                            native
                            name="status"
                            onChange={props.filterChangeStatus}
                        >
                            <option value="00">Ag.Execução</option>
                            <option value="10">Executado</option>
                            <option value="20">Não Executado</option>

                        </Select>

                    </FormControl>
                </Grid>
                <Grid item xs={1}>
                    <Tooltip title="Filtrar">
                        <IconButton
                            aria-label="Filtrar"
                            onClick={props.filterClick}
                        >
                            <FilterListIcon />
                        </IconButton>
                    </Tooltip>
                </Grid>
            </Grid>
            <br />
            <TableContainer className={classes.container}>
                <Table size="small" stickyHeader>
                    <TableHead>
                        <TableRow>
                            <TableCell
                                style={{ width: 1, display: 'none' }}
                            >
                                Id
                            </TableCell>
                            <TableCell
                                style={{ width: 1, display: 'none' }}
                            >
                                Filial
                            </TableCell>
                            <TableCell
                                style={{ width: 10 }}
                            >
                                Código
                            </TableCell>
                            <TableCell
                                style={{ width: 10 }}
                            >
                                Tipo
                            </TableCell>
                            <TableCell
                                style={{ width: 10 }}
                            >
                                Data
                            </TableCell>
                            <TableCell
                                align="center"
                                style={{ width: 10 }}
                            >
                                Status
                            </TableCell>
                            <TableCell
                                align="center"
                                style={{ width: 10 }}
                            >
                                Motivo Status
                            </TableCell>
                            <TableCell
                                style={{ width: 30 }}
                            >
                                Retorno Evento
                            </TableCell>
                            <TableCell
                                align="center"
                                style={{ width: 1 }}
                            >
                                Ação
                            </TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {props.data.map(row => (
                            <TableRow key={row.id}>
                                <TableCell
                                    style={{ width: 10, display: 'none' }}
                                >
                                    {row.id}
                                </TableCell>
                                <TableCell
                                    style={{ width: 10, display: 'none' }}
                                >
                                    {row.filial}
                                </TableCell>
                                <TableCell
                                    style={{ width: 10 }}
                                >
                                    {row.codigo}
                                </TableCell>
                                <TableCell
                                    style={{ width: 10 }}
                                >
                                    {row.tipo == 0 ? "Sincronização de Clientes" :
                                        row.tipo == 1 ? "Sincronização de Pedidos" : 
                                        row.tipo == 2 ? "Status Pedidos" :
                                        // row.tipo == 3 ? "Lead Ganho Moskit" :
                                        "Não definido"}
                                </TableCell>
                                <TableCell
                                    style={{ width: 10 }}
                                >
                                    {row.data}
                                </TableCell>
                                <TableCell
                                    align="center"
                                    style={{ width: 10 }}
                                >
                                    <Chip
                                        style={row.status == "00" ? { backgroundColor: "secondary", color: "#fff" } :
                                            row.status == "10" ? { backgroundColor: "#0431B4", color: "#fff" }
                                                : { backgroundColor: "#B40404", color: "#fff" }}
                                        size="small"
                                        label={
                                            row.status == "00" ? "Ag.Execução" :
                                                row.status == "10" ? "Executado" : "Não executado"}
                                    />
                                </TableCell>

                                <TableCell
                                    align="center"
                                    style={{ width: 10 }}
                                >
                                    {row.statusMotivo}
                                </TableCell>
                                <TableCell
                                    style={{ width: 30 }}
                                >
                                    {row.retornoEvento}
                                </TableCell>

                                <TableCell
                                    align="center"
                                    style={{ width: 1 }}
                                >
                                    <IconButton
                                        aria-label="Ações"
                                        onClick={props.menuActionClick}
                                        disabled={!props.disabled ? props.loading : props.disabled}
                                    >
                                        <MoreVertIcon />
                                    </IconButton>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
            <TablePagination
                component="div"
                labelRowsPerPage="Linhas por Pagina"
                rowsPerPageOptions={[10, 25, 50, 100]}
                page={props.page}
                count={props.totalRows}
                rowsPerPage={props.rowsPerPage}
                onChangePage={props.changePage}
                onChangeRowsPerPage={props.changeRowsPerPage}
            />
        </div>
    );
}
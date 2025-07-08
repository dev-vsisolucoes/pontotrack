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
    TablePagination, Grid
} from "@material-ui/core";

//
// Icones
//
import MoreVertIcon from '@material-ui/icons/MoreVert';
import SpeedDialIcon from '@material-ui/lab/SpeedDialIcon';
import SpeedDial from "@material-ui/lab/SpeedDial";
import FilterListIcon from '@material-ui/icons/FilterList';

// 
// Nossos importes
//
import useStyles from "../../theme";
import { Actions } from "../../utilitarios/GeneralUtilities";

export default function UsuariosLista(props) {

    //
    // Crio o tema da aplicacao, cores, fontes etc
    //    
    const classes = useStyles();

    const [speedDialOpen, setSpeedDialOpen] = React.useState(false);

    return (
        <div style={{ display: (props.action == Actions.LIST ? "block" : "none") }}>
            {/* <SpeedDial
                ariaLabel="actions"
                className={classes.speedDial}
                icon={<SpeedDialIcon />}
                direction="left"
                open={speedDialOpen}
                //hidden={speedDialHidden}
                onOpen={() => setSpeedDialOpen(false)}
                onClose={() => setSpeedDialOpen(false)}
                onClick={props.addClick}
            >
            </SpeedDial> */}
            <Menu
                anchorEl={props.menuActionAnchorRef}
                open={Boolean(props.menuActionAnchorRef)}
                onClose={props.menuActionClose}
                value={props.menuActionAnchorRef}
                keepMounted
            >
                <MenuItem onClick={props.viewClick}>Visualizar</MenuItem>
                <MenuItem onClick={props.editClick}>Editar</MenuItem>
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
                                Número
                            </TableCell>
                            <TableCell
                                style={{ width: 10}}
                            >
                                Id Agente
                            </TableCell>
                            <TableCell
                                style={{ width: 10 }}
                            >
                                Equipe
                            </TableCell>
                            <TableCell
                            // style={{ width: 10 }}
                            >
                                Usuário
                            </TableCell>
                            <TableCell
                            // style={{ width: 10 }}
                            >
                                Email
                            </TableCell>
                           
                            {/* <TableCell
                            style={{ width: 30 }}
                            >
                                Part. Roleta
                            </TableCell>                            */}
                            <TableCell
                                align="center"
                                style={{ width: 1 }}
                            >
                                Inativo
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
                                    {row.numero}
                                </TableCell>
                                <TableCell
                                    style={{ width: 10 }}
                                >
                                    {row.agenteId}
                                </TableCell>
                                <TableCell
                                    style={{ width: 10 }}
                                >
                                    {row.equipe}
                                </TableCell>
                                <TableCell
                                // style={{ width: 10 }}
                                >
                                    {row.usuario}
                                </TableCell> 
                                <TableCell
                                // style={{ width: 10 }}
                                >
                                    {row.email}
                                </TableCell>   
                               
                                <TableCell
                                    style={{ width: 1 }}
                                >
                                    <Chip
                                        size="small"
                                        label={row.inativo ? "Sim" : "Não"}
                                        color={row.inativo ? "secondary" : "default"}
                                    />
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
import React from "react";

import useStyles from "../../theme";

import { Box, Grid, TextField, Paper, Table, TableHead, TableCell, TableBody, TableRow, Button, Select, InputLabel, FormControl, FormLabel, LinearProgress } from "@material-ui/core";
import FormGroup from "@material-ui/core/FormGroup";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import Checkbox from "@material-ui/core/Checkbox";

import { Actions } from "../../utilitarios/GeneralUtilities";

export default function ConfiguracoesDetalhe(props) {

    //
    // Crio o tema da aplicacao, cores, fontes etc
    //    
    const classes = useStyles();

    return (
        <div
            style={{ display: (props.action == Actions.ADD || props.action == Actions.UPDATE || props.action == Actions.DELETE || props.action == Actions.VIEW ? "block" : "none") }}>
            <Grid container spacing={4}>
                <Grid item xs={12} sm={3}>
                    <TextField
                        label="Código"
                        onChange={(event) => {props.setInputValues({ ...props.inputValues, ["codigo"]: event.target.value })}}
                        value={props.inputValues.codigo}
                        variant="standard"
                        fullWidth
                        disabled
                    />
                </Grid>
                <Grid item xs={12} sm={3}>
                    <TextField
                        label="Nome"
                        onChange={(event) => {props.setInputValues({ ...props.inputValues, ["nome"]: event.target.value })}}
                        value={props.inputValues.nome}
                        variant="standard"
                        fullWidth
                        disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        label="Descrição"
                        onChange={(event) => {props.setInputValues({ ...props.inputValues, ["descricao"]: event.target.value })}}
                        value={props.inputValues.descricao}
                        variant="standard"
                        fullWidth
                        disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                    />
                </Grid>
                <Grid item xs={12} sm={12}>
                    <TextField
                        label="Valor"
                        onChange={(event) => {props.setInputValues({ ...props.inputValues, ["valor"]: event.target.value })}}
                        value={props.inputValues.valor}
                        variant="standard"
                        fullWidth
                        disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                    />
                </Grid>

                {/* <Grid item xs={12} sm={12}>
                    <FormControlLabel
                        control={
                            <Checkbox
                                onChange={(event) => {props.setInputValues({ ...props.inputValues, ["inativo"]: event.target.checked })}}
                                checked={props.inputValues.inativo}
                                color="primary"
                            />
                        }
                        label="Inativo"
                        disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                    />
                </Grid> */}

                <Grid container justify="flex-end">
                    <Grid item>
                        <Button
                            variant="contained"
                            color="primary"
                            className={classes.submit}
                            onClick={props.okClick}
                            disabled={!props.disabled ? props.loading : props.action == Actions.DELETE ? props.loading : props.disabled}
                        >
                            OK
                        </Button>
                        <Button
                            variant="contained"
                            color="primary"
                            className={classes.submit}
                            onClick={props.cancelClick}
                            disabled={props.loading}
                        >
                            Cancelar
                    </Button>
                    </Grid>
                </Grid>
            </Grid>
        </div >
    );
}
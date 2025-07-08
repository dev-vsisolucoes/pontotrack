import React from "react";

import useStyles from "../../theme";

import { Box, Grid, TextField, Paper, Table, TableHead, TableCell, TableBody, TableRow, Button, Select, InputLabel, FormControl, FormLabel, LinearProgress } from "@material-ui/core";
import FormGroup from "@material-ui/core/FormGroup";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import Checkbox from "@material-ui/core/Checkbox";

import { Actions } from "../../utilitarios/GeneralUtilities";

export default function UsuariosDetalhe(props) {

    //
    // Crio o tema da aplicacao, cores, fontes etc
    //    
    const classes = useStyles();

    return (
        <div
            style={{ display: (props.action == Actions.ADD || props.action == Actions.UPDATE || props.action == Actions.DELETE || props.action == Actions.VIEW ? "block" : "none") }}>
            <Grid container spacing={4}>
                <Grid item xs={12} sm={2}>
                    <TextField
                        label="Código"
                        onChange={(event) => {props.setInputValues({ ...props.inputValues, ["codigo"]: event.target.value })}}
                        value={props.inputValues.codigo}
                        variant="standard"
                        fullWidth
                        disabled
                    />
                </Grid>
                <Grid item xs={12} sm={2}>
                    <TextField
                        label="Número"
                        onChange={(event) => {props.setInputValues({ ...props.inputValues, ["numero"]: event.target.value })}}
                        value={props.inputValues.numero}
                        variant="standard"
                        fullWidth
                        disabled
                        InputLabelProps={{
                            shrink: true,
                        }}
                    />
                </Grid>
                <Grid item xs={12} sm={2}>
                    <TextField
                        label="Id Agente"
                        onChange={(event) => {props.setInputValues({ ...props.inputValues, ["agenteId"]: event.target.value })}}
                        value={props.inputValues.agenteId}
                        variant="standard"
                        fullWidth
                        disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                        InputLabelProps={{
                            shrink: true,
                        }}
                    />
                </Grid>
                 

                <Grid item xs={12} sm={3}>
                    <TextField
                        label="Usuario"
                        onChange={(event) => {props.setInputValues({ ...props.inputValues, ["usuario"]: event.target.value })}}
                        value={props.inputValues.usuario}
                        variant="standard"
                        fullWidth
                        disabled
                        InputLabelProps={{
                            shrink: true,
                        }}
                        // disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                    />
                </Grid>
                <Grid item xs={12} sm={3}>
                    <TextField
                        label="Senha"
                        onChange={(event) => {props.setInputValues({ ...props.inputValues, ["senha"]: event.target.value })}}
                        value={props.inputValues.senha}
                        variant="standard"
                        fullWidth
                        disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                        InputLabelProps={{
                            shrink: true,
                        }}
                    />
                </Grid>   

                <Grid item xs={12} sm={4}>
                    <TextField
                        label="Equipe"
                        onChange={(event) => {props.setInputValues({ ...props.inputValues, ["equipe"]: event.target.value })}}
                        value={props.inputValues.equipe}
                        variant="standard"
                        fullWidth
                        disabled
                        InputLabelProps={{
                            shrink: true,
                        }}
                    />
                </Grid>
                
                 <Grid item xs={12} sm={8}>
                    <TextField
                        label="Email"
                        onChange={(event) => {props.setInputValues({ ...props.inputValues, ["email"]: event.target.value })}}
                        value={props.inputValues.email}
                        variant="standard"
                        fullWidth
                        disabled
                        InputLabelProps={{
                            shrink: true,
                        }}
                    />
                </Grid>    

                {/* <Grid item xs={12} sm={12}>
                    <FormControlLabel
                        control={
                            <Checkbox
                                onChange={(event) => {props.setInputValues({ ...props.inputValues, ["participaRoleta"]: event.target.checked })}}
                                checked={props.inputValues.participaRoleta}
                                color="primary"
                            />
                        }
                        label="Participa da Roleta"
                        disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                    />
                </Grid> */}

                {/* <Grid item xs={12} sm={3}>
                    <FormControlLabel
                        control={
                            <Checkbox
                                onChange={(event) => {props.setInputValues({ ...props.inputValues, ["funilAnhang"]: event.target.checked })}}
                                checked={props.inputValues.funilAnhang}
                                color="primary"
                            />
                        }
                        label="Anhanguera"
                        disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                    />
                </Grid> */}

                <Grid item xs={12} sm={3}>
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
                </Grid>

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
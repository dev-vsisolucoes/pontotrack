import React from "react";

import useStyles from "../../theme";

import { Grid, TextField, 
    Button, Select, InputLabel, FormControl, 
} from "@material-ui/core";

import { Actions } from "../../utilitarios/GeneralUtilities";

export default function EventosDetalhes(props) {

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
                        onChange={(event) => { props.setInputValues({ ...props.inputValues, ["codigo"]: event.target.value }) }}
                        value={props.inputValues.codigo}
                        variant="standard"
                        fullWidth
                        disabled
                    />
                </Grid>
                <Grid item xs={12} sm={2}>
                    <FormControl
                        variant="standard"
                        className={classes.formControl}
                        fullWidth
                    >
                        <InputLabel htmlFor="standard-age-native-simple">Tipo</InputLabel>
                        <Select
                            variant="standard"
                            native
                            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["tipo"]: event.target.value }) }}
                            // disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                            disabled
                            value={props.inputValues.tipo}
                        >
                            <option value="0">Sincronização de Clientes</option>
                            <option value="1">Sincronização de Pedidos</option>
                            <option value="2">Status Pedidos</option>
                            {/* <option value="3">Lead Ganho Moskit</option> */}
                            {/* <option value="1">Outros</option> */}
                        </Select>
                    </FormControl>
                </Grid>              
                <Grid item xs={12} sm={3}>
                    <TextField
                        label="Data"
                        type="datetime"
                        onChange={(event) => { props.setInputValues({ ...props.inputValues, ["data"]: event.target.value }) }}
                        value={props.inputValues.data}
                        variant="standard"
                        fullWidth
                        disabled
                        // disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                    />
                </Grid>

                <Grid item xs={12} sm={2}>
                    <FormControl
                        variant="standard"
                        className={classes.formControl}
                        fullWidth
                    >
                        <InputLabel htmlFor="standard-age-native-simple">Status</InputLabel>
                        <Select
                            variant="standard"
                            native
                            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["status"]: event.target.value }) }}
                            //disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                            disabled
                            value={props.inputValues.status}
                        >
                            <option value="00">Ag.Execução</option>
                            <option value="10">Executado</option>
                            <option value="20">Não Executado</option>
                        </Select>
                    </FormControl>
                </Grid>

                <Grid item xs={12} sm={3}>
                    <TextField
                        label="Motivo Status"
                        onChange={(event) => { props.setInputValues({ ...props.inputValues, ["statusMotivo"]: event.target.value }) }}
                        value={props.inputValues.statusMotivo}
                        variant="standard"
                        fullWidth
                        disabled
                        // disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                    />
                </Grid>                
                <Grid item xs={12} sm={12}>
                    <TextField
                        label="Evento"
                        onChange={(event) => { props.setInputValues({ ...props.inputValues, ["evento"]: event.target.value }) }}
                        value={props.inputValues.evento}
                        variant="standard"
                        fullWidth
                        disabled
                        // disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                        multiline
                        rows="6"
                    />
                </Grid>

                <Grid item xs={12} sm={12}>
                    <TextField
                        label="Retorno do evento"
                        onChange={(event) => { props.setInputValues({ ...props.inputValues, ["retornoEvento"]: event.target.value }) }}
                        value={props.inputValues.retornoEvento}
                        variant="standard"
                        fullWidth
                        disabled
                        // disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
                        multiline
                        rows="10"
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
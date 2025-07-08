import React from "react";

import useStyles from "../../theme";

import { Box, Grid, TextField, Paper, Table, TableHead, TableCell, TableBody, TableRow, Button, Select, InputLabel, FormControl, FormLabel, LinearProgress } from "@material-ui/core";
import FormGroup from "@material-ui/core/FormGroup";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import Checkbox from "@material-ui/core/Checkbox";

import { Actions } from "../../utilitarios/GeneralUtilities";

export default function ParceiroDetalhe(props) {

  //
  // Crio o tema da aplicacao, cores, fontes etc
  //    
  const classes = useStyles();

  return (
    <div
      style={{ display: (props.action == Actions.ADD || props.action == Actions.UPDATE || props.action == Actions.DELETE || props.action == Actions.VIEW ? "block" : "none") }}>
      <Grid container spacing={2}>
        <Grid item xs={12} sm={2}>
          <TextField
            variant="standard"
            fullWidth
            label="Código"
            autoFocus
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["codigo"]: event.target.value }) }}
            value={props.inputValues.codigo}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={2}>
          <TextField
            variant="standard"
            fullWidth
            label="Número"
            autoFocus
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["numeroRef"]: event.target.value }) }}
            value={props.inputValues.numeroRef}
            disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
          />
        </Grid>
        <Grid item xs={12} sm={2}>
          <TextField
            variant="standard"
            fullWidth
            label="Tipo"
            autoFocus
            // disabled={props.action == Actions.DELETE ? true : !props.disabled ? props.loading : props.disabled}
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["tipoCad"]: event.target.value }) }}
            value={props.inputValues.tipoCad}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={6}>
          <TextField
            variant="standard"
            fullWidth
            label="Nome"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["nome"]: event.target.value }) }}
            value={props.inputValues.nome}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={2}>
          <TextField
            variant="standard"
            fullWidth
            label="Cnjp / Cpf"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["cpfCnpj"]: event.target.value }) }}
            value={props.inputValues.cpfCnpj}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={3}>
          <TextField
            variant="standard"
            fullWidth
            label="Endereco"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["endereco"]: event.target.value }) }}
            value={props.inputValues.endereco}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={2}>
          <TextField
            variant="standard"
            fullWidth
            label="Cep"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["cep"]: event.target.value }) }}
            value={props.inputValues.cep}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={4}>
          <TextField
            variant="standard"
            fullWidth
            label="Logradouro"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["logradouro"]: event.target.value }) }}
            value={props.inputValues.logradouro}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={1}>
          <TextField
            variant="standard"
            fullWidth
            label="Número"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["numero"]: event.target.value }) }}
            value={props.inputValues.numero}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={4}>
          <TextField
            variant="standard"
            fullWidth
            label="Complemento"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["complemento"]: event.target.value }) }}
            value={props.inputValues.complemento}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={3}>
          <TextField
            variant="standard"
            fullWidth
            label="Bairro"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["bairro"]: event.target.value }) }}
            value={props.inputValues.bairro}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={4}>
          <TextField
            variant="standard"
            fullWidth
            label="Cidade"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["cidade"]: event.target.value }) }}
            value={props.inputValues.cidade}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={1}>
          <TextField
            variant="standard"
            fullWidth
            label="Uf"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["uf"]: event.target.value }) }}
            value={props.inputValues.uf}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={2}>
          <TextField
            variant="standard"
            fullWidth
            label="Fone1"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["fone1"]: event.target.value }) }}
            value={props.inputValues.fone1}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={2}>
          <TextField
            variant="standard"
            fullWidth
            label="Fone2"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["fone2"]: event.target.value }) }}
            value={props.inputValues.fone2}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={2}>
          <TextField
            variant="standard"
            fullWidth
            label="Celular"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["celular"]: event.target.value }) }}
            value={props.inputValues.celular}
            disabled
          />
        </Grid>
        <Grid item xs={12} sm={6}>
          <TextField
            variant="standard"
            fullWidth
            label="Email"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["email"]: event.target.value }) }}
            value={props.inputValues.email}
            disabled
          />
        </Grid><Grid item xs={12} sm={3}>
          <FormControlLabel
            control={<Checkbox value="1" color="primary" />}
            label="Inativo"
            onChange={(event) => { props.setInputValues({ ...props.inputValues, ["inativo"]: event.target.checked }) }}
            disabled
          />
        </Grid>
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
    </div >
  );
}
import React, { useEffect, useRef, useState } from "react";

import Table from "@material-ui/core/Table";
import TableBody from "@material-ui/core/TableBody";
import TableCell from "@material-ui/core/TableCell";
import TableHead from "@material-ui/core/TableHead";
import TableRow from "@material-ui/core/TableRow";

// import FormGroup from '@material-ui/core/FormGroup';

import {
  Typography,
  ListItem,
  ListItemIcon,
  ListItemText,
  CssBaseline,
  Toolbar,
  AppBar,
  ListSubheader,
  IconButton,
  Badge,
  Drawer,
  Avatar,
  Divider,
  List,
  Container,
  Tooltip,
  TextField,
  FormControlLabel,
  FormLabel,
  FormGroup,
  Checkbox,
  Button,
  Box,
  Snackbar,
  ButtonGroup,
  Popper,
  ClickAwayListener,
  Grow,
  MenuItem,
  MenuList,
  Menu,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Fab,
  FormControl,
  InputLabel,
  Select,
  TableContainer,
} from "@material-ui/core";

import Grid from "@material-ui/core/Grid";
import Paper from "@material-ui/core/Paper";
import ChevronLeftIcon from "@material-ui/icons/ChevronLeft";
import NotificationsIcon from "@material-ui/icons/Notifications";
import ControlPointIcon from "@material-ui/icons/ControlPoint";
import DashboardIcon from "@material-ui/icons/Dashboard";
import ShoppingCartIcon from "@material-ui/icons/ShoppingCart";
import PeopleIcon from "@material-ui/icons/People";
import BarChartIcon from "@material-ui/icons/BarChart";
import LayersIcon from "@material-ui/icons/Layers";
import AssignmentIcon from "@material-ui/icons/Assignment";
import SettingsIcon from "@material-ui/icons/Settings";
import CloseIcon from '@material-ui/icons/Close';
import MenuIcon from "@material-ui/icons/Menu";
import ArrowDropDownIcon from '@material-ui/icons/ArrowDropDown';
import EditIcon from '@material-ui/icons/Edit';
import AddIcon from '@material-ui/icons/Add';
import MoreVertIcon from '@material-ui/icons/MoreVert';


import Fade from '@material-ui/core/Fade';

import useStyles from "../theme";

import Axios from "axios";

import NumberUtilities from "../utils/NumberUtilities"
import DateUtilities from "../utils/DateUtilities";

//
//Selecionar os produtos
//
export default function Formularios(props) {
  
  const [state, setState] = React.useState({
    brises: false,
    acm: false,
    pisoElevado: false,
    forro: false,
    brisePVC: false,
    piso: false,
    carpete: false,
    drywall: false,
    reforma: false,
    estruturaMetalica: false,
    projeto: false,
    moveisCorporativos: false,
    divisoriasDPT: false,
    outros: false,
  });

  //
  // Crio o tema da aplicacao, cores, fontes etc
  //
  const classes = useStyles();

  //
  // Acoes possiveis em uma tela
  //
  const Actions = {
    LIST: 0, //Lista de dados
    ADD: 1, //Grava um registro
    UPDATE: 2, //Atualiza um registro
    DELETE: 3 //Excluir um registro
  };

  //
  //
  //
  const [action, setAction] = useState(0); // Variavel para controle da acao que ocorre na tela
  const [openDialog, setOpenDialog] = useState(false); // para o controle da dialog
  const [openDialogKey, setOpenDialogKey] = useState(); // para o controle da dialog
  const [data, setData] = useState([]); // Variavel que mantem a lista de dados
  const [snackBarOpen, setSnackBarOpen] = useState({ open: false, message: "" }); // Isso controla a caixa de mensagem, para quando ocorre um erro
  const [anchorEl, setAnchorEl] = useState(null); // Uso isso para ancorar o menu popup do botao editar a um botao EDITAR
  const [disabled, setDisabled] = useState(false);

  //
  // Aqui controlamos todos os inputs da tela
  //
  const [inputValues, setInputValues] = useState({
    id: 0,
    filial: 0,
    codigo: 0,
    nome: "",
    sobrenome: "",
    empresa: "",
    email: "",
    celular: "",
    fixo: "",
    cidade: "",
    uf: "",
    nomeObra: "",
    produto: "",
    obs: "",
    anexo: "",
    status: "00",
  });

  const [selected, setSelected] = React.useState([]);

  const colourStyles = {
    control: styles => ({ ...styles, backgroundColor: 'white' }),
    container: styles => ({ ...styles, width: 300 })
  };

  //
  // Quando se altera um campo na tela, persisto ele no state do react
  //
  const inputHandleChange = (event) => {

    let v;
    if (event.target.type === "checkbox") {
      v = event.target.checked;

    } else if (event.target.type === "file") {
      v = event.target.files;

    } else {
      v = event.target.value;
    }

    const { name, value } = event.target;
    setInputValues({ ...inputValues, [name]: v });
  };

  //
  //Enviar registro do formulário
  //
  const sendHandleClick = (event) => {

    setDisabled(true);

    var formdata = new FormData();

    for (var prop in inputValues) {
      formdata.append(prop, inputValues[prop]);
    }

    formdata.append("produto", selected);
    //formdata.append("anexo[]", inputValues["anexo"][0]);
    //formdata.append("anexo[]", inputValues["anexo"][1]);
    
    // inputValues["anexo"].map(element => {
    //   console.log(element);
    // });
   
    for(let x=0; x<inputValues["anexo"].length; x++) {
      formdata.append("anexo[]", inputValues["anexo"][x]);
    }

    fetch(props.config.service + "formulario?XDEBUG_SESSION_START=netbeans-xdebug", {
      method: 'POST',
      body: formdata
    }).then((res) => {
      res.json().then((res) => {

        if (res.Code === 200) {

          setState([]);
          setInputValues({
            id: 0,
            filial: 0,
            codigo: 0,
            nome: "",
            sobrenome: "",
            empresa: "",
            email: "",
            celular: "",
            fixo: "",
            cidade: "",
            uf: "",
            nomeObra: "",
            produto: "",
            obs: "",
            anexo: "",
            status: "00",
          });

          setSnackBarOpen({
            open: true,
            message: "Dados enviados com sucesso"
          });

          setDisabled(false);

        } else {
          setSnackBarOpen({
            open: true,
            message: res.Message
          });

          setDisabled(false);
        }
      })
    });
  }

  //
  // Rotina para fechar a caixa de mensagem de erro
  //
  const snakeBar_HandleClose = (event, reason) => {
    setSnackBarOpen({
      open: false,
      message: ""
    });
  };

  const handleCloseDialog = (event) => {
    setOpenDialog(false);
  }

  const produtoHandleChange = (event) => {
    let value = event.currentTarget.value;

    let newSelected = [];

    const selectedIndex = selected.indexOf(value);

    if (selectedIndex === -1) {
      newSelected = newSelected.concat(selected, value);

    } else if (selectedIndex === 0) {
      newSelected = newSelected.concat(selected.slice(1));

    } else if (selectedIndex === selected.length - 1) {
      newSelected = newSelected.concat(selected.slice(0, -1));

    } else if (selectedIndex > 0) {
      newSelected = newSelected.concat(
        selected.slice(0, selectedIndex),
        selected.slice(selectedIndex + 1),
      );
    }

    setSelected(newSelected);
    setState({ ...state, [event.target.name]: event.target.checked });
  }

  return (
    <div>
      {/* <Paper className={classes.paper}>
        <Toolbar>
          <Typography
            className={classes.title}
            variant="h6"
            id="tableTitle"
            component="div"
            color="primary"
          >
            {props.title}
          </Typography>
        </Toolbar>
        <form className={classes.form} >
          <Grid container spacing={2}>
            <Grid item xs={12} sm={2}>
              <TextField
                // variant="standard"
                variant="outlined"
                fullWidth
                label="Nome"
                name="nome"
                onChange={inputHandleChange}
                value={inputValues.nome}
                //disabled={disabled}
                required
              />
            </Grid>
            <Grid item xs={12} sm={2}>
              <TextField
                // variant="standard"
                variant="outlined"
                fullWidth
                label="Sobrenome"
                name="sobrenome"
                onChange={inputHandleChange}
                value={inputValues.sobrenome}
              //disabled={disabled}
              // required
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField
                variant="outlined"
                fullWidth
                label="Email"
                name="email"
                onChange={inputHandleChange}
                value={inputValues.email}
                //disabled={disabled}
                required
              />
            </Grid>
            <Grid item xs={12} sm={2}>
              <TextField
                variant="outlined"
                fullWidth
                label="Celular"
                name="celular"
                onChange={inputHandleChange}
                value={inputValues.celular}
              //disabled={disabled}
              // required
              />
            </Grid>
            <Grid item xs={12} sm={2}>
              <TextField
                variant="outlined"
                fullWidth
                label="Fixo"
                name="fixo"
                onChange={inputHandleChange}
                value={inputValues.fixo}
                //disabled={disabled}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                variant="outlined"
                fullWidth
                label="Empresa"
                name="empresa"
                onChange={inputHandleChange}
                value={inputValues.empresa}
              />
            </Grid>
            <Grid item xs={12} sm={3}>
              <TextField
                variant="outlined"
                fullWidth
                label="Cidade"
                name="cidade"
                onChange={inputHandleChange}
                value={inputValues.cidade}
                
                required
              />
            </Grid>
            <Grid item xs={12} sm={3}>
              <FormControl
                variant="outlined"
                className={classes.FormControlLabel}
                fullWidth
                required
              >
                <InputLabel htmlFor="standard-age-native-simple">Estado</InputLabel>
                <Select
                  variant="outlined"
                  
                  native
                  
                  name="uf"
                  onChange={inputHandleChange}
                  value={inputValues.uf}
                >                  
                  <option value=""></option>
                  <option value="147372">AC</option>
                  <option value="147373">AL</option>
                  <option value="147375">AP</option>
                  <option value="147374">AM</option>
                  <option value="147376">BA</option>
                  <option value="147377">CE</option>
                  <option value="147379">ES</option>
                  <option value="147380">GO</option>
                  <option value="147381">MA</option>
                  <option value="147384">MT</option>
                  <option value="147383">MS</option>
                  <option value="147382">MG</option>
                  <option value="147385">PA</option>
                  <option value="147386">PB</option>
                  <option value="147365">PR</option>
                  <option value="147387">PE</option>
                  <option value="147388">PI</option>
                  <option value="147389">RJ</option>
                  <option value="147390">RN</option>
                  <option value="147366">RS</option>
                  <option value="147391">RO</option>
                  <option value="147392">RR</option>
                  <option value="147367">SC</option>
                  <option value="147394">SP</option>
                  <option value="147393">SE</option>
                  <option value="147395">TO</option>
                  <option value="147378">DF</option>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={12}>
              <TextField
                variant="outlined"
                fullWidth
                label="Nome da Obra"
                name="nomeObra"
                onChange={inputHandleChange}
                value={inputValues.nomeObra}
              />
            </Grid>
            <Grid item xs={12} sm={12}>
              <FormLabel component="legend">Produto / Serviço*</FormLabel>
              <FormGroup row>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={state.brises}
                      onChange={produtoHandleChange}
                      name="brises"
                      color="primary"
                      value="147413"
                    />
                  }
                  label="Brises"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={state.acm}
                      onChange={produtoHandleChange}
                      name="acm"
                      color="primary"
                      value="147412"
                    />
                  }
                  label="ACM"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={state.pisoElevado}
                      onChange={produtoHandleChange}
                      name="pisoElevado"
                      color="primary"
                      value="147421"
                    />
                  }
                  label="Piso Elevado"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={state.forro}
                      onChange={produtoHandleChange}
                      name="forro"
                      color="primary"
                      value="147417"
                    />
                  }
                  label="Forro"
                />                
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={state.piso}
                      onChange={produtoHandleChange}
                      name="piso"
                      color="primary"
                      value="147420"
                    />
                  }
                  label="Piso"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={state.carpete}
                      onChange={produtoHandleChange}
                      name="carpete"
                      color="primary"
                      value="147414"
                    />
                  }
                  label="Carpete"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={state.drywall}
                      onChange={produtoHandleChange}
                      name="drywall"
                      color="primary"
                      value="147415"
                    />
                  }
                  label="Drywall"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={state.reforma}
                      onChange={produtoHandleChange}
                      name="reforma"
                      color="primary"
                      value="147423"
                    />
                  }
                  label="Reforma"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={state.estruturaMetalica}
                      onChange={produtoHandleChange}
                      name="estruturaMetalica"
                      color="primary"
                      value="147416"
                    />
                  }
                  label="Estrutura Metálica"
                />
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={state.projeto}
                      onChange={produtoHandleChange}
                      name="projeto"
                      color="primary"
                      value="147422"
                    />
                  }
                  label="Projeto"
                />                
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={state.outros}
                      onChange={produtoHandleChange}
                      name="outros"
                      color="primary"
                      value="147419"
                    />
                  }
                  label="Outros"
                />
              </FormGroup>
            </Grid>

            <Grid item xs={12} sm={12}>
              <TextField
                variant="outlined"
                fullWidth
                label="Observações"
                name="obs"
                onChange={inputHandleChange}
                value={inputValues.obs}
              //disabled={disabled}
              />
            </Grid>
            <Grid item xs={12} sm={12}>
              <FormLabel component="legend">Anexar seu Projeto / Fotos:</FormLabel><br />
              <input
                type="file"
                name="anexo"
                multiple
                onChange={inputHandleChange}
              />
            </Grid>
          </Grid>
          <Grid container justify="flex-start">
            <Grid item>
              <Button
                variant="contained"
                color="primary"
                className={classes.submit}
                onClick={sendHandleClick}
                disabled={disabled}
              >
                ENVIAR
              </Button>
            </Grid>
          </Grid>
        </form>
      </Paper>

      <Snackbar
        open={snackBarOpen.open}
        //backgroundColor={snackBarOpen.background}
        //severity="success"
        anchorOrigin={{
          vertical: 'top',
          horizontal: 'center',
        }}
        onClose={snakeBar_HandleClose}
        //TransitionComponent={state.Transition}
        TransitionComponent={Fade}
        message={snackBarOpen.message}
        autoHideDuration={3000}
        action={
          <React.Fragment>
            <IconButton size="small" aria-label="close" color="inherit" onClick={snakeBar_HandleClose}>
              <CloseIcon fontSize="small" />
            </IconButton>
          </React.Fragment>
        }
      /> */}
    </div>
  );
}

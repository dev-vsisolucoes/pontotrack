import React, { useEffect, useRef, useState, Fragment } from "react";
// import React from "react";
import Avatar from "@material-ui/core/Avatar";
import Button from "@material-ui/core/Button";
import CssBaseline from "@material-ui/core/CssBaseline";
import TextField from "@material-ui/core/TextField";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import Checkbox from "@material-ui/core/Checkbox";
import Link from "@material-ui/core/Link";
import Grid from "@material-ui/core/Grid";
import Box from "@material-ui/core/Box";
import LockOutlinedIcon from "@material-ui/icons/LockOutlined";
import Typography from "@material-ui/core/Typography";
import { makeStyles } from "@material-ui/core/styles";
import Container from "@material-ui/core/Container";
import Autocomplete from '@material-ui/lab/Autocomplete';
import KSnackBar from "./components/ksnackbar";
import ApiUtilities from "./utilitarios/ApiUtilities";
import main from "./main";

import {
  FormControl,
  InputLabel,
  Select,
  Slider,
  TableContainer,
  Snackbar,
  Fade,
  IconButton,
  LinearProgress
} from "@material-ui/core";

import CloseIcon from '@material-ui/icons/Close';
import { withRouter } from "react-router-dom";

function Copyright() {
  return (
    <Typography variant="body2" color="textSecondary" align="center">
      {"Copyright © "}
      <Link color="inherit" href="http://www.vsisolucoes.com.br/">
        VSI Soluções
      </Link>{" "}
      {new Date().getFullYear()}
      {"."}
    </Typography>
  );
}

const useStyles = makeStyles(theme => ({
  main: {
    backgroundColor: "#ffffff"
  },
  paper: {
    marginTop: theme.spacing(8),
    display: "flex",
    flexDirection: "column",
    alignItems: "center"
  },
  avatar: {
    margin: theme.spacing(3),
    backgroundColor: theme.palette.secondary.main
  },
  form: {
    width: "100%", // Fix IE 11 issue.
    marginTop: theme.spacing(1)
  },
  submit: {
    margin: theme.spacing(3, 0, 2)
  }
}));

function Login(props) {
  const [snackBar, setSnackBar] = useState({ open: false, message: "" });
  const [snackBarOpen, setSnackBarOpen] = useState({ open: false, message: "" }); // Isso controla a caixa de mensagem, para quando ocorre um erro

  const [inputValues, setInputValues] = useState({
    usuario: '',
    senha: '',
    // funil:'',
    empresa: "0",
  });

  const classes = useStyles();

  // 
  //Variavel pra Controlar o disable dos Botões
  //
  const [disabled, setDisabled] = React.useState(false);

  const [loading, setLoading] = useState(false);


  const handleLogIn = () => {

    setLoading(true);

    var formdata = new FormData();
    for (var prop in inputValues) {
      formdata.append(prop, inputValues[prop]);
    }

    ApiUtilities.fetch(props.config.service + "usuario/login", inputValues, "POST")
      .then((res) => {
        res.json().then((res) => {

          if (res.Code === 200) {

            props.history.push("/visao-geral");

          } else {
            setSnackBarOpen({
              open: true,
              message: res.Message
            });

            setLoading(false);
          }
        }).catch((error) => {
          setSnackBarOpen({
            open: true,
            message: "Erro de comunicação: " + error.message
          });
          setLoading(false);
        });

        setDisabled(false);
      });
  };

  const snakeBarClose = (event) => {
    setSnackBar({
      open: false,
      message: ""
    });
  };

  return (
    <Container
      component="main"
      maxWidth="xs"
      className={classes.main}
    >
      <CssBaseline />
      <div className={classes.paper}>
        <main
          inputValues={inputValues}
          setInputValues={setInputValues}
        />
        <Avatar className={classes.avatar}>
          <LockOutlinedIcon />
        </Avatar>
        <Typography component="h1" variant="h5">
          Acesso ao sistema
        </Typography>

        <TextField
          variant="outlined"
          margin="normal"
          required
          fullWidth
          id="usuario"
          label="Usuário"
          autoComplete="email"
          autoFocus
          onChange={(event) => { setInputValues({ ...inputValues, ["usuario"]: event.target.value }) }}
          disabled={!disabled ? loading : disabled}
        />

        <TextField
          variant="outlined"
          margin="normal"
          required
          fullWidth
          id="senha"
          label="Senha"
          type="password"
          autoComplete="current-password"
          onChange={(event) => { setInputValues({ ...inputValues, ["senha"]: event.target.value }) }}
          disabled={!disabled ? loading : disabled}
        />

        <br />
        <Select
          fullWidth
          variant="outlined"
          label="Empresa"
          native
          onChange={(event) => { setInputValues({ ...inputValues, ["empresa"]: event.target.value }) }}
          value={inputValues.empresa}
          disabled={!disabled ? loading : disabled}
        >
          <option value="0">PONTOTRACK</option>
          {/* <option value="2">MTC</option> */}

        </Select>

        <Button
          fullWidth
          variant="contained"
          color="primary"
          id="entrar"
          className={classes.submit}
          onClick={handleLogIn}
          disabled={!disabled ? loading : disabled}
        >
          Entrar
          </Button>

        {loading && <div> <br /><LinearProgress /> </div>}

      </div>
      <br />
      {loading && <LinearProgress />}

      <Box mt={8}>
        <Copyright />
      </Box>

      <KSnackBar
        options={[snackBarOpen, setSnackBarOpen]}
      />
    </Container>
  );
}

export default withRouter(Login);
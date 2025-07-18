import React, { useEffect, useState } from "react";

import {
  Typography,
  Toolbar,
  LinearProgress,
} from "@material-ui/core";

import Paper from "@material-ui/core/Paper";

//
// Outros importações
// 
import useStyles from "../../theme";

import { Actions } from "../../utilitarios/GeneralUtilities";
import KSnackBar from "../../components/ksnackbar";
import ApiUtilities from "../../utilitarios/ApiUtilities";

import ParceiroLista from "./parceiros-lista";
import ParceiroDetalhe from "./parceiros-detalhes";

export default function Parceiros(props) {

  //
  // Crio o tema da aplicacao, cores, fontes etc
  //
  const classes = useStyles();

  const service = props.config.service + "parceiro";

  //
  // Definição das variaveis usando(react-hook)
  //
  const [action, setAction] = useState(Actions.LIST); // Variavel para controle da acao que ocorre na tela

  const [anchorEl, setAnchorEl] = React.useState(null); // Uso isso para andescricaoar o menu popup do botao editar a um botao EDITAR

  const [snackBarOpen, setSnackBarOpen] = useState({ // variavel para controlar a caixa de mensagem, para quando ocorre algum alerta
    open: false,
    message: ""
  });

  const [inputValues, setInputValues] = useState({ // Rook para controle dos inputs na tela
    id: 0,
    filial: 0,
    codigo: 0,
    numeroRef: 0,
    tipoCad: "",
    nome: "",
    cpfCnpj: "",
    endereco: "",
    cep: "",
    logradouro: "",
    numero: "",
    complemento: "",
    bairro: "",
    cidade: "",
    uf: "",
    pais: "",
    fone1: "",
    fone2: "",
    celular: "",
    email: "",
  });

  const [data, setData] = React.useState([]); // Variavel que mantem a lista de dados

  const [disabled, setDisabled] = React.useState(false);

  const [loading, setLoading] = useState(false);

  //
  // Controle da paginacao
  //
  const [page, setPage] = React.useState(0);
  const [rowsPerPage, setRowsPerPage] = React.useState(10);
  const [totalRows, setTotalRows] = React.useState(0);

  const [filter, setFilter] = React.useState("");

  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  // INICIO: EVENTOS DA TELA

  //
  // Chamada para atualizar lista de dados, toda vez que algo muda
  //
  useEffect(() => {
    list(page, rowsPerPage);
  }, [setData]);

  //
  // Evento para novo registro
  //
  const addHandleClick = (event) => {

    setDisabled(false);

    setInputValues({
      id: 0,
      filial: 0,
      codigo: 0,
      numeroRef: 0,
      tipoCad: "",
      nome: "",
      cpfCnpj: "",
      endereco: "",
      cep: "",
      logradouro: "",
      numero: "",
      complemento: "",
      bairro: "",
      cidade: "",
      uf: "",
      pais: "",
      fone1: "",
      fone2: "",
      celular: "",
      email: "",
    });

    setAction(Actions.ADD);

    setDisabled(false);

    setLoading(false);
  }

  //
  // Logica do botao EDITAR
  //
  const editHandleClick = (event) => {

    find(Actions.UPDATE);

    setAnchorEl(null);

    setDisabled(false);
  };

  //
  // Logica do botao EXCLUIR
  //
  const deleteHandleClick = (event) => {
    find(Actions.DELETE);

    setAnchorEl(null);

    setDisabled(true);
  }

  //
  // Logica para VISUALIZAR
  //
  const viewHandleClick = (event) => {
    find(Actions.VIEW);

    setAnchorEl(null);

    setDisabled(true);
  }

  //
  // Evento quando se tenta persistir um registro, quando-se clica no ok da tela
  //
  const okHandleClick = (event) => {
    confirm();
  }

  //
  // Evento para o botao fechar da tela
  // 
  const closeHandleClick = () => {
    list(page, rowsPerPage);

    setAction(Actions.LIST);
  }

  // FIM: EVENTOS DA TELA
  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  // FUNCOES UTILITARIAS

  //
  // Rotina carrega os dados a partir do back-end
  //
  async function confirm() {

    setLoading(true);

    let method = "POST";

    if (action == Actions.UPDATE) {
      method = "PUT";

    } else if (action == Actions.DELETE) {
      method = "DELETE";
    }

    await ApiUtilities.fetch(service, inputValues, method, {numeroRef : inputValues.numeroRef})
      .then((res) => {
        res.json().then((res) => {

          //Caso de tudo certo atualizo a lista
          if (res.Code === 200) {

            if (action == Actions.ADD) {

              setInputValues({
                id: 0,
                filial: 0,
                codigo: 0,
                numeroRef: 0,
                tipoCad: "",
                nome: "",
                cpfCnpj: "",
                endereco: "",
                cep: "",
                logradouro: "",
                numero: "",
                complemento: "",
                bairro: "",
                cidade: "",
                uf: "",
                pais: "",
                fone1: "",
                fone2: "",
                celular: "",
                email: "",
              });

              setSnackBarOpen({
                open: true,
                message: res.Message
                // message: "Operação executada com sucesso"
              });

            } else {

              list(page, rowsPerPage);

              setAction(Actions.LIST);

            }

            setSnackBarOpen({
              open: true,
              message: res.Message
            });

            //Caso contrario exibo a mensagem de erro
          } else {
            setSnackBarOpen({
              open: true,
              message: res.Message
            });
          }

          setLoading(false);
        })
      }).catch((error) => {
        setSnackBarOpen({
          open: true,
          message: "Erro de comunicação: " + error.message
        });

        setLoading(false);
      });
  }

  async function list(page, rowsPerPage) {

    setLoading(true);

    setDisabled(false);

    await ApiUtilities.fetch(service, {page: page , limit: rowsPerPage , filter : filter}, "GET")
      .then((res) => {
        res.json().then((res) => {

          //Caso de tudo certo atualizo a lista
          if (res.Code === 200) {

            setData(res.Content.rows);

            setTotalRows(res.Content.totalRows);

            setPage(page);

            setRowsPerPage(rowsPerPage);

            //Caso contrario exibo a mensagem de erro
          } else {
            setSnackBarOpen({
              open: true,
              message: res.Message
            });
          }

          setLoading(false);
        })
      }).catch((error) => {
        setSnackBarOpen({
          open: true,
          message: "Erro de comunicação: " + error.message
        });

        setLoading(false);
      });
  };

  async function find(action) {

    setLoading(true);

    let id = anchorEl.parentElement.parentElement.cells[0].innerText;

    ApiUtilities.fetch(service + "/find", { id: id }, "GET")
    .then((res) => {
      res.json().then((res) => {

        //Caso de tudo certo atualizo a lista
        if (res.Code === 200) {

          let model = res.Content;

          setInputValues({
            id: model.id || "",
            filial: model.filial || 0,
            codigo: model.codigo || 0,
            numeroRef: model.numeroRef || 0,
            tipoCad: model.tipoCad || "",
            nome: model.nome || "",
            cpfCnpj: model.cpfCnpj || "",
            endereco: model.endereco || "",
            cep: model.cep || 0,
            logradouro: model.logradouro || "",
            numero: model.numero || 0,
            complemento: model.complemento || "",
            bairro: model.bairro || "",
            cidade: model.cidade || "",
            uf: model.uf || "",
            pais: model.pais || "",
            fone1: model.fone1 || 0,
            fone2: model.fone2 || 0,
            celular: model.celular || 0,
            email: model.email || "",
          });

          setAction(action);

          //Caso contrario exibo a mensagem de erro
        } else {
          setSnackBarOpen({
            open: true,
            message: res.Message
          });
        }

        // setDisabled(false);

        setLoading(false);
      })
    }).catch((error) => {
      setSnackBarOpen({
        open: true,
        message: "Erro de comunicação: " + error.message
      });

      setLoading(false);
    });
  }

  return (
    <div>
      <Paper className={classes.paper}>
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

        <ParceiroLista
          action={action}
          data={data}
          menuActionAnchorRef={anchorEl}
          menuActionClick={(event) => { setAnchorEl(event.currentTarget) }}
          menuActionClose={(event) => { setAnchorEl(null) }}
          addClick={addHandleClick}
          editClick={editHandleClick}
          deleteClick={deleteHandleClick}
          viewClick={viewHandleClick}
          disabled={disabled}
          loading={loading}
          filterClick={() => { list(page, rowsPerPage); }}
          filterChange={(event) => { setFilter(event.target.value) }}
          page={page}
          totalRows={totalRows}
          rowsPerPage={rowsPerPage}
          changePage={(event, newPage) => { list(newPage, rowsPerPage); }}
          changeRowsPerPage={(event) => { list(0, parseInt(event.target.value)); }}
        />

        <ParceiroDetalhe
          action={action}
          inputValues={inputValues}
          setInputValues={setInputValues}
          okClick={okHandleClick}
          cancelClick={closeHandleClick}
          disabled={disabled}
          loading={loading}
        />
        <br />
        {loading && <LinearProgress />}
      </Paper>

      <KSnackBar
        options={[snackBarOpen, setSnackBarOpen]}
      />
    </div>
  );
}
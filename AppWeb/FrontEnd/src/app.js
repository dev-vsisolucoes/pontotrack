import React from "react";

import { Switch, Route, Router } from "react-router";
import { BrowserRouter } from "react-router-dom";

import DashboardIcon from "@material-ui/icons/Dashboard";
import SettingsIcon from "@material-ui/icons/Settings";
import PeopleIcon from "@material-ui/icons/People";
import ListAltIcon from '@material-ui/icons/ListAlt';

import Login from "./login";
import Main from "./main";
// import Configuracoes from "./geral/configuracoes";
// import VisaoGeral from "./geral/visao-geral";
// import Usuarios from "./geral/usuarios";

import Configuracoes from "./general/configuracoes/configuracoes";
import VisaoGeral from "./general/visao-geral";
import Usuarios from "./general/usuarios/usuarios";

import Formulario from "./general/formulario";

export default function App(props) {

    const modules = [
      {
        path: "/visao-geral", 
        ui: <VisaoGeral {...props} title="Visão Geral" />,
        title: "Visão Geral", 
        tooltip: "Visão Geral",
        icon: <DashboardIcon />
      },
      {
        path: "/configuracoes", 
        ui: <Configuracoes {...props} title="Configurações" />,
        title: "Configurações", 
        tooltip: "Configurações",
        icon: <SettingsIcon />
      },
      {
        path: "/usuarios", 
        ui: <Usuarios {...props} title="Usuários" />,
        title: "Usuários", 
        tooltip: "Usuários",
        icon: <PeopleIcon />
      },
      // {
      //   path: "/form", 
      //   ui: <Formulario {...props} title="Formulário" />,
      //   title: "Formulário", 
      //   tooltip: "Formulário",
      //   icon: <ListAltIcon />
      // }
    ];
    
    return (
    <BrowserRouter basename={props.config.baseName}>
      <Switch>
        <Route key="/" exact path="/" render={() => <Login {...props} />} />
        <Route key="/formulario" exact path="/formulario" render={() => <Formulario {...props} />} />
        {
          modules.map(
            item => <Route key={item.path} exact path={item.path} render={(props) => <Main {...props} ui={item.ui} modules={modules} />} />
          )
        }
      </Switch>
    </BrowserRouter>
  );
}

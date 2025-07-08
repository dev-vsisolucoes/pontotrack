import React from "react";

import clsx from "clsx";
import { makeStyles } from "@material-ui/core/styles";
import Table from "@material-ui/core/Table";
import TableBody from "@material-ui/core/TableBody";
import TableCell from "@material-ui/core/TableCell";
import TableHead from "@material-ui/core/TableHead";
import TableRow from "@material-ui/core/TableRow";
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
  Checkbox,
  Button,
  Box
} from "@material-ui/core";

import ExitToAppIcon from "@material-ui/icons/ExitToApp";

import MenuIcon from "@material-ui/icons/Menu";

import { Link, NavLink } from "react-router-dom";

//import Title from "./Title";

import useStyles from "./theme";

export default function Main(props) {
  const classes = useStyles();

  const [open, setOpen] = React.useState(false);

  const handleDrawerOpen = () => {
    setOpen(true);
  };

  const handleDrawerClose = () => {
    setOpen(false);
  };

  const handleLogOff = () => {
    props.history.push("/");
  };

  //const fixedHeightPaper = clsx(classes.paper, classes.fixedHeight);

  return (
    <div className={classes.root}>
      <CssBaseline />
      <AppBar 
        position="absolute"
        className={clsx(classes.appBar, open && classes.appBarShift)}
      >
        <Toolbar className={classes.toolbar}>
          {/* <Avatar className={classes.avatar} /> */}
          {/* <Avatar className={classes.avatar} src="/static/img/avatar/1.jpg" /> */}
          <Typography
            component="h1"
            variant="h6"
            color="inherit"
            noWrap
            className={classes.appTitle}
          >
            PONTOTRACK
          </Typography>  
          <IconButton 
            color="inherit" 
            onClick={handleLogOff}
          >
            {/* <Badge badgeContent={4} color="secondary">
              <NotificationsIcon />
            </Badge> */}
            <ExitToAppIcon />
          </IconButton>                  
        </Toolbar>
      </AppBar>
      <Drawer
        variant="permanent"
        classes={{paper: clsx(classes.drawerPaper, !open && classes.drawerPaperClose)}}
        //open={open}
      >
        {/* <Avatar className={classes.avatar} src="/static/img/avatar/1.jpg" /> */}
        {/* <p /> */}
        {/* <Divider /> */}
        <List>
          {/* <MainListItems /> */}
          {
            props.modules.map(item => (
              <ListItem key={item.path} button component={Link} to={item.path}>
                <ListItemIcon>
                  <Tooltip title={item.tooltip} placement="right"> 
                      {/* <DashboardIcon /> */}
                      {item.icon}
                  </Tooltip>          
                </ListItemIcon>
                <ListItemText primary={item.title} />
              </ListItem>)
            )
          }
        </List>
      </Drawer>            
      <main className={classes.content}>
        <div className={classes.appBarSpacer} />
        <Container maxWidth="xl" className={classes.container}>
          {props.ui}
          <Box pt={4}>{/* <Copyright /> */}</Box>
        </Container>
      </main>      
    </div>
  );
}

import { makeStyles } from "@material-ui/core/styles";

const drawerWidth = 240;

const useStyles = makeStyles(theme => ({
  root: {
    display: "flex",
    width: '100%'
  },
  toolbar: {
    //height: 40,
    paddingRight: 10 // keep right padding when drawer closed
  },
  toolbarIcon: {
    display: "flex",
    alignItems: "center",
    justifyContent: "flex-end",
    padding: "0 8px",
    ...theme.mixins.toolbar
  },
  appBar: {
    height: 55,
    //zIndex: theme.zIndex.drawer + 1,
    transition: theme.transitions.create(["width", "margin"], {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.leavingScreen
    })
  },
  appBarShift: {
    marginLeft: drawerWidth,
    width: `calc(100% - ${drawerWidth}px)`,
    transition: theme.transitions.create(["width", "margin"], {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.enteringScreen
    })
  },
  menuButton: {
    marginRight: 36
  },
  menuButtonHidden: {
    display: "none"
  },
  appTitle: {
    flexGrow: 1,
    marginTop: theme.spacing(-1),
    marginLeft: theme.spacing(7)
  },  
  title: {
    flexGrow: 1,
    marginTop: theme.spacing(-1),
    marginLeft: theme.spacing(-2)
  },
  drawerPaper: {
    //marginTop: 56,
    //height: "92%",
    position: "relative",
    whiteSpace: "nowrap",
    width: drawerWidth,
    transition: theme.transitions.create("width", {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.enteringScreen
    })
  },
  drawerPaperClose: {
    overflowX: "hidden",
    transition: theme.transitions.create("width", {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.leavingScreen
    }),
    width: theme.spacing(7), //isso aqui esconde o painel lateral
    [theme.breakpoints.up("sm")]: {
      width: theme.spacing(7)
    }
  },
  appBarSpacer: theme.mixins.toolbar,
  content: {
    flexGrow: 1,
    height: "100vh",
    overflow: "auto"
  },
  container: {
    paddingTop: theme.spacing(4),
    paddingBottom: theme.spacing(4)
  },
  paper: {
    marginTop: theme.spacing(2),
    marginLeft: theme.spacing(7),
    padding: theme.spacing(2),
    display: "flex",
    overflow: "auto",
    flexDirection: "column"
  },
  fixedHeight: {
    height: 240
  },
  avatar: {
    //alignItems: "center",
    marginTop: theme.spacing(-1),
    marginLeft: theme.spacing(-2),
    //marginBottom: theme.spacing(1)
  },
  addButton: {
    marginRight: theme.spacing(-3)
  },
  submit: {
    margin: theme.spacing(3, 1, 2)
  },
  fab: {
    position: 'absolute',
    bottom: theme.spacing(2),
    right: theme.spacing(2),
  },  
  container: {
    maxHeight: 440,
  },  
  paperItens: {
    marginTop: theme.spacing(0),
    marginLeft: theme.spacing(0),
    marginBottom: theme.spacing(2),
    padding: theme.spacing(2),
    display: "flex",
    overflow: "auto",
    flexDirection: "column"
  },
  speedDial: {
    position: 'absolute',
    bottom: theme.spacing(2),
    right: theme.spacing(2),
  }
}));

export default useStyles;

import { createTheme } from "@mui/material/styles";

export const neon = {
  magenta: "#ff2fd3",
  cyan: "#00f0ff",
  green: "#39ff88",
  amber: "#ffcc33",
  red: "#ff3b5c",
};

export const theme = createTheme({
  palette: {
    mode: "dark",
    primary: { main: neon.cyan, contrastText: "#04050a" },
    secondary: { main: neon.magenta, contrastText: "#04050a" },
    success: { main: neon.green },
    warning: { main: neon.amber },
    error: { main: neon.red },
    background: {
      default: "#0a0912",
      paper: "#12111c",
    },
    text: {
      primary: "#e6f7ff",
      secondary: "#8f96b8",
    },
  },
  shape: { borderRadius: 10 },
  typography: {
    fontFamily: "'Rajdhani', 'Segoe UI', sans-serif",
    h1: { fontFamily: "'Orbitron', sans-serif", fontWeight: 700, letterSpacing: 1 },
    h2: { fontFamily: "'Orbitron', sans-serif", fontWeight: 700, letterSpacing: 1 },
    h6: { fontFamily: "'Orbitron', sans-serif", fontWeight: 600, letterSpacing: 0.5 },
    button: {
      fontFamily: "'Orbitron', sans-serif",
      fontWeight: 600,
      letterSpacing: 1.5,
      textTransform: "uppercase",
    },
  },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: {
          backgroundImage:
            "radial-gradient(circle at 20% 20%, rgba(0,240,255,0.08), transparent 40%), " +
            "radial-gradient(circle at 80% 0%, rgba(255,47,211,0.08), transparent 45%), " +
            "linear-gradient(rgba(255,255,255,0.03) 1px, transparent 1px), " +
            "linear-gradient(90deg, rgba(255,255,255,0.03) 1px, transparent 1px)",
          backgroundSize: "auto, auto, 40px 40px, 40px 40px",
          minHeight: "100svh",
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: "none",
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          position: "relative",
          transition: "transform 120ms ease, box-shadow 120ms ease",
          "&:active": {
            transform: "scale(0.94)",
          },
        },
      },
      variants: [
        {
          props: { variant: "contained", color: "primary" },
          style: {
            boxShadow: `0 0 10px ${neon.cyan}55, 0 0 0 1px ${neon.cyan}66 inset`,
            "&:hover": {
              boxShadow: `0 0 22px ${neon.cyan}aa, 0 0 0 1px ${neon.cyan} inset`,
            },
          },
        },
        {
          props: { variant: "contained", color: "secondary" },
          style: {
            boxShadow: `0 0 10px ${neon.magenta}55, 0 0 0 1px ${neon.magenta}66 inset`,
            "&:hover": {
              boxShadow: `0 0 22px ${neon.magenta}aa, 0 0 0 1px ${neon.magenta} inset`,
            },
          },
        },
      ],
    },
  },
});

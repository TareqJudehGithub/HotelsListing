import { Container } from "@mui/material";
import CountriesList from "./features/country/CountriesList";

function App() {
	return (
		<Container maxWidth="xl" sx={{ mt: 14 }}>
			<CountriesList />
		</Container>
	);
}

export default App;

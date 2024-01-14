import React, { lazy, Suspense } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { IApiPaths, setApiPaths } from '../shared/lib/apiConstants';

const CreateGamePage = lazy(() => import('./CreateGame/CreateGamePage'));
const TakePartPage = lazy(() => import('./TakePart/TakePartPage'));
const PostCoordinator = lazy(() => import('./PostCoordinator/PostCoordinatorPage'));
export interface IAppProps {
    basename: string;
    apiPaths: IApiPaths;
}
const App = (props: IApiPaths) => {
    setApiPaths(props.apiPaths)
    
    return (
        <Suspense fallback={
            <div>
                <h1>Vennligst vent....</h1>
            </div>
        }>
            <Router basename={props.basename}>
                <RouteApp />
            </Router>
        </Suspense>
    )
}

export const RouteApp = () => {
    return (
        <Router>
            <Route path="/" element={CreateGamePage}/>
            <Route path="/Delta" element={TakePartPage}/>
            <Route path="/PostAnsvarlig" element={PostCoordinator}/>
        </Router>
    )
}

export default App;